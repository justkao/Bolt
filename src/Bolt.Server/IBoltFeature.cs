namespace Bolt.Server
{
    public interface IBoltFeature
    {
        ServerActionContext ActionContext { get; set; }

        /// <summary>
        /// Gets or sets <see cref="ISerializer"/> assigned to current context.
        /// </summary>
        ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets <see cref="ISerializer"/> assigned to current context.
        /// </summary>
        IExceptionWrapper ExceptionWrapper { get; set; }

        /// <summary>
        /// Gets or sets <see cref="BoltServerOptions"/> assigned to current context.
        /// </summary>
        BoltServerOptions Options { get; set; }

        /// <summary>
        /// Gets or sets <see cref="BoltServerOptions"/> assigned to current context.
        /// </summary>
        IServerErrorHandler ErrorHandler{ get; set; }

        /// <summary>
        /// Gets or sets <see cref="IParameterBinder"/> assigned to current context.
        /// </summary>
        IParameterBinder ParameterBinder { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IResponseHandler"/> assigned to current context.
        /// </summary>
        IResponseHandler ResponseHandler { get; set; }
    }
}