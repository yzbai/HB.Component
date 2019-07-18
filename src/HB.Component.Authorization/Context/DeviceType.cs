using System;

namespace HB.Component.Authorization.Abstractions
{
    internal enum DeviceType
    {
        None = 0,
        Android = 1,
        Iphone = 2,
        Web = 3,
        Postman = 4
    }

    internal static class DeviceTypeChecker
    {
        public static DeviceType Check(string clientType)
        {
            if (Enum.TryParse<DeviceType>(clientType, out DeviceType result))
            {
                if (Enum.IsDefined(typeof(DeviceType), result))
                {
                    return result;
                }
            }

            return DeviceType.None;
        }
    }
}
