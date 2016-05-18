using System;

#if !FEATURE_SERVICEMODEL_SERVER

namespace System.ServiceModel
{
    public class OperationContractAttribute : Attribute
    {
    }

    public class ServiceContractAttribute : Attribute
    {
    }
}

#endif


