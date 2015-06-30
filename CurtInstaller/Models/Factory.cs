using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurtInstaller.Models
{
    public static class Factory
    {

       static InstallModel installModel;
        public static InstallModel GetInstallModel()
        {
            if (installModel == null)
            {
                installModel = new InstallModel();
            }
            return installModel;
        }
    }
}
