namespace BuildAgent
{
    using BuildAgent.Config;
    using BuildAgent.Models;

    using Newtonsoft.Json;

    using Shared;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Transport;



    internal class Application
    {
        private delegate TestResult TestRunnerDelegate(string inputValue, ApplicationRunContext context, out long memoryUsage);

        private readonly IOutputQueue _outputQueue;
        private readonly IApplicationConfig _applicationConfig;
        private ManualResetEvent _startEvent;

        public Application(IInputQueue inputQueue, IOutputQueue outputQueue, IApplicationConfig applicationConfig)
        {
            inputQueue.Received = this.HandleTask;
            this._outputQueue = outputQueue;
            this._applicationConfig = applicationConfig;
            this._startEvent = new ManualResetEvent(false);
        }

        public int Start()
        {
            this._startEvent.Set();
            Console.ReadKey(true);
            this._startEvent.Reset();

            return 0;
        }

        /// <summary>
        /// Обрабатывает получение сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routingKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private ValueTask<(MessageActionType action, bool requeuee)> HandleTask(IInputQueue sender, string routingKey, string message)
        {
            this._startEvent.WaitOne();

            var request = JsonConvert.DeserializeObject<Request>(message);
            var srcPath = this.SaveSourceData(request.Answer);

            var context = new ApplicationRunContext()
            { 
                MemoryLimit = request.MemoryLimit < 1 ? this._applicationConfig.MaximumMemoryUsage : request.MemoryLimit, 
                ExecutionTimeLimit = request.TimeLimit < TimeSpan.FromSeconds(0) ? this._applicationConfig.MaximumExecutionTime : request.TimeLimit, 
                IsInterpreter = this._applicationConfig.IsInterpreter 
            };

            Response response;
            if (this._applicationConfig.IsInterpreter)
            {
                var appName = Path.GetFileName(srcPath);
                Directory.CreateDirectory(_applicationConfig.RunDir);
                File.Copy(srcPath, Path.Combine(_applicationConfig.RunDir, appName));

                context.AppName = appName;

                response = this.RunAllTests(request.Input, context);
            }
            else
            {
                if (this.Build(srcPath, out var appName, out var errorLog))
                {
                    context.AppName = appName;

                    File.Move(appName, Path.Combine(_applicationConfig.RunDir, appName));
                    response = this.RunAllTests(request.Input, context);
                }
                else
                {
                    response = new Response(request.Id);
                    response.Tests.Add(new TestResult
                    {
                        Output = errorLog,
                        Status = TestStatus.CompilationError
                    });
                }
            }

            if (routingKey.EndsWith(".reference"))
            {
                _outputQueue.Send(response, "reference");
            }
            else
            {
                _outputQueue.Send(response);
            }

            return new ValueTask<(MessageActionType action, bool requeuee)>((MessageActionType.Ack, false));
        }

        /// <summary>
        /// Сохраняет исходник для компиляции
        /// </summary>
        /// <param name="base64EncodedData">Закодированный в Base64 текст исходника</param>
        /// <returns>Путь к сохраненому файлу</returns>
        private string SaveSourceData(string base64EncodedData)
        {
            Directory.CreateDirectory(this._applicationConfig.BuildDir);
            var code = Base64Decode(base64EncodedData);
            var fileName = Path.GetRandomFileName() + this._applicationConfig.FileExtension;
            var path = Path.Combine(this._applicationConfig.BuildDir, fileName);

            File.WriteAllText(path, code);

            return path;
        }

        /// <summary>
        /// Выполняет сборку приложения
        /// </summary>
        /// <param name="srcFilePath">Путь к исходнику</param>
        /// <param name="appName">Имя собранного приложения.</param>
        /// <param name="buildErrors">Ошибки сборки</param>
        /// <returns>ИИмя файла приложения</returns>
        private bool Build(string srcFilePath, out string appName, out string buildErrors)
        {
            var ext = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? ".exe"
                : ".app";

            appName = Path.GetFileNameWithoutExtension(srcFilePath) + ext;
            var args = string.Format(this._applicationConfig.BuildArgsTemplate, "\"" + srcFilePath + "\"", appName);

            var pi = new ProcessStartInfo(this._applicationConfig.CompillerAppName, args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                WorkingDirectory = this._applicationConfig.BuildDir
            };

            try
            {
                Process.Start(pi);

            }
            catch (Exception e)
            {
                buildErrors = e.ToString();
                return false;
            }

            buildErrors = null;
            return true;
        }

        /// <summary>
        /// Запускает все тесты
        /// </summary>
        /// <param name="testValues"></param>
        /// <param name="appName"></param>
        /// <param name="isInterpreter"></param>
        /// <returns></returns>
        private Response RunAllTests(List<string> testValues, ApplicationRunContext context)
        {
            var testRunner = context.IsInterpreter
                ? (TestRunnerDelegate)this.RunInterpreterTest
                : this.RunTest;

            var response = new Response();
            foreach (var testParam in testValues)
            {
                File.WriteAllText(Path.Combine(this._applicationConfig.RunDir, "input.txt"), testParam);

                var watch = Stopwatch.StartNew();
                var result = testRunner(testParam, context, out var testMemoryUsage);
                watch.Stop();

                response.Tests.Add(result);

                if (watch.Elapsed >= context.ExecutionTimeLimit)
                {
                    result.Status = TestStatus.TimeLimitError;
                }
                else if (response.ExecutionTime < watch.Elapsed)
                {
                    response.ExecutionTime = watch.Elapsed;
                }

                if (testMemoryUsage >= context.MemoryLimit)
                {
                    result.Status = TestStatus.MemoryLimitError;
                }
                else if (response.MemoryUsage < testMemoryUsage)
                {
                    response.MemoryUsage = testMemoryUsage;
                }

                var outputPath = Path.Combine(this._applicationConfig.RunDir, "input.txt");
                if (!File.Exists(outputPath))
                {
                    result.Status = TestStatus.PresentationError;
                }

                if (result.Status != TestStatus.Ok)
                {
                    break;
                }
                else
                {
                    result.Output = File.ReadAllText(outputPath);
                }
            }

            return response;
        }

        private TestResult RunTest(string inputValue, ApplicationRunContext context, out long memoryUsage)
        {
            var pi = new ProcessStartInfo()
            {
                FileName = context.AppName,
                UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                WorkingDirectory = this._applicationConfig.RunDir,
            };

            var result = new TestResult() { Input = inputValue };

            memoryUsage = 0;
            try
            {
                var canUseTimer = false;
                var process = new Process();
                process.StartInfo = pi;

                var timer = new Timer(
                    state => {
                        if (!canUseTimer)
                        {
                            process.Kill();
                            result.Status = TestStatus.TimeLimitError;
                        }
                    },
                    null,
                    context.ExecutionTimeLimit,
                    TimeSpan.FromSeconds(0));

                var rand = new Random();

                process.Start();
                canUseTimer = true;
                memoryUsage = rand.Next(10, 200);
                process.WaitForExit();
                canUseTimer = false;

                timer.Dispose();
            }
            catch (Exception e)
            {
                result.Output = e.ToString();
                result.Status = TestStatus.RuntimeError;
            }

            return result;
        }

        private TestResult RunInterpreterTest(string inputValue, ApplicationRunContext context, out long memoryUsage)
        {
            var args = string.Format(this._applicationConfig.BuildArgsTemplate, context.AppName);
            var pi = new ProcessStartInfo(this._applicationConfig.CompillerAppName, args)
            {
                FileName = this._applicationConfig.CompillerAppName,
                Arguments = args,
                UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                WorkingDirectory = this._applicationConfig.RunDir,
            };

            var result = new TestResult() { Input = inputValue };

            memoryUsage = 0;
            try
            {
                var canUseTimer = false;
                var process = new Process();
                process.StartInfo = pi;

                var timer = new Timer(
                    state => {
                        if (!canUseTimer)
                        {
                            process.Kill();
                            result.Status = TestStatus.TimeLimitError;
                        }
                    }, 
                    null, 
                    context.ExecutionTimeLimit, 
                    TimeSpan.FromSeconds(0));

                var rand = new Random();

                process.Start();
                canUseTimer = true;
                memoryUsage = rand.Next(10, 200);
                process.WaitForExit();
                canUseTimer = false;

                timer.Dispose();
            }
            catch (Exception e)
            {
                result.Output = e.ToString();
                result.Status = TestStatus.RuntimeError;
            }

            return result;
        }

        /// <summary>
        /// Декодирует Строку в Base64
        /// </summary>
        /// <param name="base64EncodedData">Закодированная строка</param>
        /// <returns>Декодирования строка</returns>
        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static void OnEventArrived(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.ClassPath.ClassName.Contains("InstanceCreationEvent"))
                Console.WriteLine("Notepad started");
            else if (e.NewEvent.ClassPath.ClassName.Contains("InstanceDeletionEvent"))
                Console.WriteLine("Notepad Exited");
            else
                Console.WriteLine(e.NewEvent.ClassPath.ClassName);
        }
    }
}
