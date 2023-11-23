using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MultiCam
{
    internal class CameraSettings
    {
        //A (modified) definition of OleCreatePropertyFrame found here: http://groups.google.no/group/microsoft.public.dotnet.languages.csharp/browse_thread/thread/db794e9779144a46/55dbed2bab4cd772?lnk=st&q=[DllImport(%22olepro32.dll%22)]&rnum=1&hl=no#55dbed2bab4cd772
        [System.Runtime.InteropServices.DllImport("oleaut32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true)]
        public static extern int OleCreatePropertyFrame(
            IntPtr hwndOwner,
            int x,
            int y,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpszCaption,
            int cObjects,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Interface, ArraySubType=System.Runtime.InteropServices.UnmanagedType.IUnknown)]
            ref object ppUnk,
            int cPages,
            IntPtr lpPageClsID,
            int lcid,
            int dwReserved,
            IntPtr lpvReserved);

        public static void CAMERA_SETTINGS(DsDevice dsDevice)
        {
            CAMERA_SETTINGS(CreateFilter(dsDevice));
        }

        public static void CAMERA_SETTINGS(IBaseFilter dev)
        {
            //Get the ISpecifyPropertyPages for the filter
            ISpecifyPropertyPages pProp = dev as ISpecifyPropertyPages;
            int hr;

            if (pProp == null)
            {
                //If the filter doesn't implement ISpecifyPropertyPages, try displaying IAMVfwCompressDialogs instead!
                IAMVfwCompressDialogs compressDialog = dev as IAMVfwCompressDialogs;
                if (compressDialog != null)
                {

                    hr = compressDialog.ShowDialog(VfwCompressDialogs.Config, IntPtr.Zero);
                    DsError.ThrowExceptionForHR(hr);
                }
                return;
            }

            //Get the name of the filter from the FilterInfo struct
            FilterInfo filterInfo;
            hr = dev.QueryFilterInfo(out filterInfo);
            DsError.ThrowExceptionForHR(hr);

            // Get the propertypages from the property bag
            DsCAUUID caGUID;
            hr = pProp.GetPages(out caGUID);
            DsError.ThrowExceptionForHR(hr);

            // Create and display the OlePropertyFrame
            object oDevice = (object)dev;
            //hr = OleCreatePropertyFrame(this.Handle, 0, 0, filterInfo.achName, 1, ref oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero);
            //hr = OleCreatePropertyFrame(new System.Windows.Interop.WindowInteropHelper(this).Handle, 0, 0, filterInfo.achName, 1, ref oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero);

            //hr = OleCreatePropertyFrame(new System.Windows.Interop.WindowInteropHelper(window).Handle, 0, 0, filterInfo.achName, 1, ref oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero);           
            hr = OleCreatePropertyFrame(0, 0, 0, filterInfo.achName, 1, ref oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);

            // Release COM objects
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(caGUID.pElems);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pProp);
            if (filterInfo.pGraph != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(filterInfo.pGraph);
            }
        }

        static IBaseFilter CreateFilter(string friendlyname)
        {
            object source = null;
            Guid iid = typeof(IBaseFilter).GUID;
            foreach (DsDevice device in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                if (device.Name.CompareTo(friendlyname) == 0)
                {
                    device.Mon.BindToObject(null, null, ref iid, out source);
                    break;
                }
            }
            return (IBaseFilter)source;
        }

        static IBaseFilter CreateFilter(DsDevice dsDevice)
        {
            Guid iid = typeof(IBaseFilter).GUID;
            dsDevice.Mon.BindToObject(null, null, ref iid, out object source);
            return (IBaseFilter)source;
        }

        public static Dictionary<int, DsDevice> GetDevices()
        {
            Dictionary<int, DsDevice> devices = new Dictionary<int, DsDevice>();

            var rawdevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < rawdevices.Length; i++)
            {
                DsDevice device = rawdevices[i];
                try
                {
                    devices.Add(i, device);
                }
                catch (Exception ex)
                {
                    devices.Add(i, null);
                }
            }
            return devices;
        }
    }
}
