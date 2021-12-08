using NetSPF.Common.Attributes;

namespace NetSPF
{
    public enum SpfQualifier
    {
        /// <summary>
        /// Matching mechanisms result is a pass.
        /// </summary>
        [MappedText("+")] 
        Pass,
        
        /// <summary>
        /// Matching mechanisms result is a failure.
        /// </summary>
        [MappedText("-")] 
        Fail,
        
        /// <summary>
        /// Matching mechanisms result is a softfail.
        /// </summary>
        [MappedText("~")] 
        SoftFail,
        
        /// <summary>
        /// Matching mechanisms result is neutral.
        /// </summary>
        [MappedText("?")] 
        Neutral
    }
}