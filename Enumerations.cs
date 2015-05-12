
namespace BlueCats.Wallet
{
    public enum eWalletDataTypes : byte
    {
        CardBalanceRequest = 0,
        CardRedmeptionRequest = 1,
        TransactionNotification = 2
    }

    public enum eWalletErrors : byte
    {
        SerializationFailed = 1,
        JSONInvalid,
        RequestTypeMissing,
        RequestTypeNotSupported,
        MerchantIDMissing,
        CardNotFound,
        CardBarcodeMissing,
        CardBalanceInsufficient,
        TransactionIDMissing,
        TransactionNotFound,
        TransactionCanceled,
        TransactionCompleted,
        MerchantNotSupported,
        SpecifiedAmountExceedsRemainingAmount
    };
}
