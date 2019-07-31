namespace NetSPF
{
    public enum SpfQualifier
    {
        /// <summary>
        /// Matching mechanisms result is a pass.
        /// </summary>
        Pass,
        
        /// <summary>
        /// Matching mechanisms result is a failure.
        /// </summary>
        Fail,
        
        /// <summary>
        /// Matching mechanisms result is a softfail.
        /// </summary>
        SoftFail,
        
        /// <summary>
        /// Matching mechanisms result is neutral.
        /// </summary>
        Neutral
    }
}