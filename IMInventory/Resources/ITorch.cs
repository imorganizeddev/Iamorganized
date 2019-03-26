namespace Organized.Torch
{
    using System;

    public interface ITorch : IDisposable
    {
        bool SupportedByDevice{ get; }
        bool On{ get; }
        void Toggle ();
        event TorchToggledEventHandler Toggled;
    }
}