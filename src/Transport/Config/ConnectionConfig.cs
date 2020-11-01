namespace Transport.Config
{
    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;

    public class ConnectionConfig : IEquatable<ConnectionConfig>
    {
        /// <summary>
        /// IP-адрес либо наименование хоста
        /// </summary>
        public readonly string Host = "localhost";

        /// <summary>
        /// Номер порта для подключения
        /// </summary>
        public readonly int Port = 5672;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public readonly string Login;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public readonly string Password;

        /// <summary>
        /// Виртуальный хост для доступа во время соединения
        /// </summary>
        public readonly string VirtualHost;

        public readonly ushort PrefetchSize;

        /// <summary>
        /// Конструктор класса <see cref="ConnectionConfig"/>
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <param name="server">Адрес сервера</param>
        /// <param name="port">Порт сервера</param>
        /// <param name="virtualHost">Виртуальный хост</param>
        [JsonConstructor]
        public ConnectionConfig(
            string login,
            string password = null,
            string host = null,
            int? port = null,
            string virtualHost = null,
            ushort prefetchSize = 1)
        {
            if (!string.IsNullOrEmpty(login))
            {
                this.Login = login;
            }

            if (!string.IsNullOrEmpty(password))
            {
                this.Password = password;
            }

            if (!string.IsNullOrEmpty(host))
            {
                this.Host = host;
            }

            if (port.HasValue)
            {
                this.Port = port.Value;
            }

            this.VirtualHost = string.IsNullOrEmpty(virtualHost)
                ? $"{this.Login}.vhost"
                : virtualHost;

            this.PrefetchSize = prefetchSize;
        }

        /// <summary>
        /// Оператор сравнения конфигураций соединения
        /// </summary>
        /// <param name="config1">Первый конфиг</param>
        /// <param name="config2">Второй конфиг</param>
        /// <returns>True, если объекты равны</returns>
        public static bool operator ==(ConnectionConfig config1, ConnectionConfig config2)
        {
            return EqualityComparer<ConnectionConfig>.Default.Equals(config1, config2);
        }

        /// <summary>
        /// Оператор сравнения конфигураций соединения
        /// </summary>
        /// <param name="config1">Первый конфиг</param>
        /// <param name="config2">Второй конфиг</param>
        /// <returns>True, если объекты НЕ равны</returns>
        public static bool operator !=(ConnectionConfig config1, ConnectionConfig config2)
        {
            return !(config1 == config2);
        }

        /// <summary>
        /// Проверяет объекты на равенство
        /// </summary>
        /// <param name="obj">Объект, с которым происходит сравнение</param>
        /// <returns>True, если объекты равны</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ConnectionConfig);
        }

        /// <summary>
        /// Проверяет объекты на равенство
        /// </summary>
        /// <param name="other">Объект, с которым происходит сравнение</param>
        /// <returns>True, если объекты равны</returns>
        public bool Equals(ConnectionConfig other)
        {
            return other != null &&
                   this.Host == other.Host &&
                   this.Port == other.Port &&
                   this.Login == other.Login &&
                   this.Password == other.Password &&
                   this.VirtualHost == other.VirtualHost;
        }

        /// <summary>
        /// Формирует хеш код объекта
        /// </summary>
        /// <returns>Хеш код объекта</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Host, this.Port, this.Login, this.Password, this.VirtualHost);
        }

        /// <summary>
        /// Приводит объект к удобному для отображения виду
        /// </summary>
        /// <returns>Строка для отображения</returns>
        public override string ToString()
        {
            return $"{this.Host};{this.Port};{this.VirtualHost};{this.Login};{this.Password}";
        }
    }
}
