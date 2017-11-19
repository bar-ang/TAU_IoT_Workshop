using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
namespace RPiRunner2
{
    /// <summary>
    /// Links between the users and the hardware.
    /// This class expands the basic LinearHX class and allow to inform whether the weight-scale is being used, and provide information about the current user or previous uses.
    /// </summary>
    class UserHardwareLinker : LinearHX
    {
        private TempProfile currUser;
        public const int WEIGH_AVG = 1000;
        public const float CALIB_FACTOR = 1.75f;

        public UserHardwareLinker(GpioPin sck, GpioPin dout): base(dout,sck)
        {
            currUser = null;
        }

        public TempProfile currentServedUser()
        {
            return currUser;
        }

        public void StartUser(TempProfile user)
        {
            currUser = user;
        }

        public void FinishUser()
        {
            currUser = null;
        }

        public bool CheckOrigin(DRP drp)
        {
            return drp.Token == currentServedUser().Token && drp.UserName.Equals(currentServedUser().Username);
        }
    }
    class TempProfile
    {
        private string username;
        private ulong token;
        private long appid;

        public TempProfile(string username, ulong token, long appid)
        {
            this.username = username;
            this.token = token;
            this.appid = appid;
        }

        public string Username { get => username; set => username = value; }
        public ulong Token { get => token; set => token = value; }
        public long Appid { get => appid; set => appid = value; }
    }
}
