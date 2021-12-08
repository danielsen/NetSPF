using System;

namespace NetSPF.Common.Text
{
    public interface ITokenEnumeratorCheckpoint : IDisposable
    {
        /// <summary>
        /// Rollback to the checkpoint;
        /// </summary>
        void Rollback();
    }
}