namespace Organized.Torch
{
    using Android.Hardware;
    using System;

    public delegate void TorchToggledEventHandler();

    public class Torch : ITorch
    {
        public event TorchToggledEventHandler Toggled;

        protected virtual void OnToggled()
        {
            if (Toggled != null)
                Toggled();
        }

        const string FLASH_MODE_TORCH = Android.Hardware.Camera.Parameters.FlashModeTorch;
        const string FLASH_MODE_ON = Android.Hardware.Camera.Parameters.FlashModeOn;
        const string FLASH_MODE_OFF = Android.Hardware.Camera.Parameters.FlashModeOff;

        private Camera _deviceCamera;
        private Camera DeviceCamera
        {
            get
            {
                if (_deviceCamera == null)
                    _deviceCamera = Camera.Open();

                return _deviceCamera;
            }
        }

        private Camera.Parameters _parameters;
        private Camera.Parameters Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = DeviceCamera.GetParameters ();

                return _parameters;
            }
        }

        private string _supportdFlashModeOn;
        private string SupportedFlashModeOn {
            get
            {
                if (_supportdFlashModeOn == null)
                {
                    var supportedModes = Parameters.SupportedFlashModes;

                    if(supportedModes.Contains(FLASH_MODE_TORCH))
                        _supportdFlashModeOn = FLASH_MODE_TORCH;
                    else if(supportedModes.Contains(FLASH_MODE_ON))
                        _supportdFlashModeOn = FLASH_MODE_ON;
                }
                return _supportdFlashModeOn;
            }
        }

        public bool On {
            get
            {
                return Parameters.FlashMode == SupportedFlashModeOn;
            }
        }

        private bool? _supportedByDevice;
        public bool SupportedByDevice
        {
            get
            {
                if (_supportedByDevice == null) {
                    _supportedByDevice = SupportedFlashModeOn != null;
                }

                return _supportedByDevice.Value;
            }
        }

        public void Toggle()
        {
			
            Parameters.FlashMode = On ? FLASH_MODE_OFF : SupportedFlashModeOn;
            DeviceCamera.SetParameters(Parameters);
			IMInventory.IMApplication.camera = DeviceCamera;
            OnToggled();
        }

        public void Dispose ()
        {
            try {
                if(_deviceCamera != null)
                {
                    _supportdFlashModeOn = null;
                    _parameters = null;
                    _supportedByDevice = null;

                    _deviceCamera.Release();
                    _deviceCamera.Dispose();
                }
            } catch (Exception) {

            }
            _deviceCamera = null;
        }
    }
}

