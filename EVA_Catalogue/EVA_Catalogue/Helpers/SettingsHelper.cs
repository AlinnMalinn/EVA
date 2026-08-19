using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVA_Catalogue
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

        public void SetTypeOfDevice(string typeOfDevice)
        {
            TypeOfDevice = typeOfDevice;
        }
    }
}
