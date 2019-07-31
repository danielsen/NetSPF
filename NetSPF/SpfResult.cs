namespace NetSPF
{
    /// <summary>
    /// The result of a SPF evaluation
    /// </summary>
    public enum SpfResult
    {
        /// <summary>
        /// Either no valid DNS label was extracted from the SMTP data
        /// or no SPF records were retrieved from DNS.
        /// </summary>
        None,
        
        /// <summary>
        /// "Neutral" results indicate that the ADMD is explicitly stating that
        /// it makes no assertion about the authorization status of the identity (IP).
        /// </summary>
        Neutral,
        
        /// <summary>
        /// "Pass" indicates an explicit statement that the client is authorized
        /// to use the domain in the given identity. 
        /// </summary>
        Pass,
        
        /// <summary>
        /// "Fail" indicates an explicit statement that the client is not authorized
        /// to use the domain in the given identity.
        /// </summary>
        Fail,
        
        /// <summary>
        /// "Softfail" indicates a weak assertion that the client is probably not authorized
        /// to use the domain in the given identity.
        /// </summary>
        SoftFail,
        
        /// <summary>
        /// "Temperror" indicates a transient error in verification. Usually DNS related.
        /// </summary>
        TemporaryError,
        
        /// <summary>
        /// "Permerror" indicates that the domain's published records cannot be interpreted.
        /// </summary>
        PermanentError
    }
}