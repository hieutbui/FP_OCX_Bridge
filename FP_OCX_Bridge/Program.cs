using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FP_OCX_Bridge
{
    internal class Program
    {
        static AxFP_CLOCKLib.AxFP_CLOCK pOcxObject;

        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length < 4)
            {
                NData data;
                data.success = false;
                data.message = "Missing device info";
                data.function = "Initialization";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return 1;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int result = -1;
            using (var form = new Form())
            {
                form.ShowInTaskbar = false;
                form.WindowState = FormWindowState.Minimized;
                form.ShowIcon = false;
                form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new System.Drawing.Point(-2000, -2000);

                pOcxObject = new AxFP_CLOCKLib.AxFP_CLOCK();
                ((System.ComponentModel.ISupportInitialize)pOcxObject).BeginInit();
                form.Controls.Add(pOcxObject);
                ((System.ComponentModel.ISupportInitialize)pOcxObject).EndInit();

                form.Load += (s, e) => form.Hide();

                form.Shown += (s, e) =>
                {
                    int machineNum = int.Parse(args[0]);
                    string ip = args[1];
                    int port = int.Parse(args[2]);
                    int password = int.Parse(args[3]);

                    result = RunAll(machineNum, ip, port, password);
                    form.Close();
                };

                Application.Run(form);
            }
            return result;
        }

        static int RunAll(int machineNum, string ip, int port, int password)
        {
            // 1. Connect
            int connectResult = Connect(machineNum, ip, port, password);
            if (connectResult != 0)
                return connectResult;
            // 2. Read logs
            int logResult = ReadAllGLogData(machineNum);

            // 3. Disconnect (always attempt, even if log read failed)
            Disconnect();

            return logResult;
        }

        static int Connect(int machineNum, string ip, int port, int password)
        {
            try
            {
                bool bRet = pOcxObject.SetIPAddress(ref ip, port, password);

                NData data;

                if (!bRet)
                {
                    data.success = false;
                    data.message = "Failed to set IP address";
                    data.function = "SetIPAddress";
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                    return -1;
                }

                bRet = pOcxObject.OpenCommPort(machineNum);

                if (!bRet)
                {
                    data.success = false;
                    data.message = "Failed to open communication port";
                    data.function = "Connect";
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                    return -1;
                }

                data.success = true;
                data.message = null;
                data.function = "Connect";

                Console.WriteLine(JsonConvert.SerializeObject(data));
                return 0;
            }
            catch (Exception ex)
            {
                NData data;
                data.success = false;
                data.message = ex.Message;
                data.function = "Connect";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return -1;
            }
        }

        static int Disconnect()
        {
            try
            {
                pOcxObject.CloseCommPort();
                NData data;
                data.success = true;
                data.message = "Disconnected successfully";
                data.function = "Disconnect";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return 0;
            }
            catch (Exception ex)
            {
                NData data;
                data.success = false;
                data.message = ex.Message;
                data.function = "Disconnect";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return -1;
            }
        }

        static int EnableDevice(int MachineNum)
        {
            try
            {
                bool result = pOcxObject.EnableDevice(MachineNum, 1);
                NData data;
                if (result)
                {
                    data.success = true;
                    data.message = "Enable device successfully";
                    data.function = "EnableDevice";
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                    return 0;
                }
                else
                {
                    data.success = false;
                    data.message = "Failed to enable device";
                    data.function = "EnableDevice";
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                    return -1;
                }
            }
            catch (Exception ex)
            {
                NData data;
                data.success = false;
                data.message = ex.Message;
                data.function = "EnableDevice";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return -1;
            }
        }

        static int DisableDevice(int MachineNum)
        {
            try
            {
                bool result = pOcxObject.EnableDevice(MachineNum, 0);
                NData data;
                if (result)
                {
                    data.success = true;
                    data.message = "Disable device successfully";
                    data.function = "DisableDevice";
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                    return 0;
                }
                else
                {
                    data.success = false;
                    data.message = "Failed to disable device";
                    data.function = "DisableDevice";
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                    return -1;
                }
            }
            catch (Exception ex)
            {
                NData data;
                data.success = false;
                data.message = ex.Message;
                data.function = "DisableDevice";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return -1;
            }
        }

        static int ReadAllGLogData(int machineNum)
        {
            try
            {
                DisableDevice(machineNum);

                var logs = new List<GeneralLogInfo>();
                bool bRet = pOcxObject.ReadAllGLogData(machineNum);
                NData data;

                if (!bRet)
                {
                    data.success = false;
                    data.message = "Failed to read general log data";
                    data.function = "ReadAllGLogData";
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                    return -1;
                }

                // Use a new instance for each log to avoid reference issues
                do
                {
                    GeneralLogInfo gLogInfo = new GeneralLogInfo();
                    bRet = pOcxObject.GetAllGLogData(machineNum,
                        ref gLogInfo.dwTMachineNumber,
                        ref gLogInfo.dwEnrollNumber,
                        ref gLogInfo.dwEMachineNumber,
                        ref gLogInfo.dwVerifyMode,
                        ref gLogInfo.dwYear,
                        ref gLogInfo.dwMonth,
                        ref gLogInfo.dwDay,
                        ref gLogInfo.dwHour,
                        ref gLogInfo.dwMinute
                    );

                    if (bRet)
                    {
                        logs.Add(gLogInfo);
                    }
                } while (bRet);

                EnableDevice(machineNum);

                data.success = true;
                data.message = JsonConvert.SerializeObject(logs);
                data.function = "ReadAllGLogData";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return 0;
            }
            catch (Exception ex)
            {
                EnableDevice(machineNum);

                NData data;
                data.success = false;
                data.message = ex.Message;
                data.function = "ReadAllGLogData";
                Console.WriteLine(JsonConvert.SerializeObject(data));
                return -1;
            }
        }

        public struct NData
        {
            public bool success;
            public string message;
            public string function;
        }

        public struct GeneralLogInfo
        {
            public int dwTMachineNumber;
            public int dwEnrollNumber;
            public int dwEMachineNumber;
            public int dwVerifyMode;
            public int dwYear;
            public int dwMonth;
            public int dwDay;
            public int dwHour;
            public int dwMinute;
        }
    }
}