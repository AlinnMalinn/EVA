using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVA_CatalogueManual
{
    public class SettingsHelper
    {
        private static SettingsHelper _instance;  // Единственный экземпляр
        public string TypeOfDevice { get; set; }

        private SettingsHelper() { } // Закрываем конструктор, чтобы никто не мог создать объект

        public static SettingsHelper Instance
        {
            get
            {
                if (_instance == null)  // Если объект еще не создан — создаем
                    _instance = new SettingsHelper();
                return _instance;       // Возвращаем один и тот же объект
            }
        }

        public string SetTypeOfDevice(string typeOfDevice)
        {
            if (typeOfDevice == MainViewModel.ModularCircuitBreakers)
            TypeOfDevice ="*"+ MainViewModel.ModularCircuitBreakersSettings;
            else if (typeOfDevice == MainViewModel.ModularResidualCurrentCircuitBreakers)
                TypeOfDevice = "*" + MainViewModel.ModularResidualCurrentCircuitBreakersSettings;
            return TypeOfDevice;
        }
    }
}
