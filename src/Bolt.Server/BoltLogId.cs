namespace Bolt.Server
{
    public static class BoltLogId
    {
        public const int BaseError = 0;

        public const int ExceptionSerializationError = BaseError + 1;
        public const int DetailedServerErrorProcessingFailed = BaseError + 2;
        public const int HandleBoltRootError = BaseError + 3;
        public const int HandleContractMetadataError = BaseError + 4;
        public const int RequestExecutionError = BaseError + 5;
        public const int RequestExecutionTime = BaseError + 6;
        public const int RequestCancelled = BaseError + 7;
        public const int ContractNotFound = BaseError + 8;
        public const int ContractAdded = BaseError + 9;
        public const int BoltRegistration = BaseError + 10;
    }
}