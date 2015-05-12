﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCats.Serial.Examples.Wallet
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

    public static class WalletConstants
    {
        public const string WALLET_DATA_TYPE_TINY_KEY = @"type";
        public const string WALLET_MERCHANT_ID_TINY_KEY = @"mid";
        public const string WALLET_CARD_BARCODE_TINY_KEY = @"code";
        public const string WALLET_CARD_OPENING_BALANCE_TINY_KEY = @"obal";
        public const string WALLET_CARD_CURRENT_BALANCE_TINY_KEY = @"cbal";
        public const string WALLET_TRANSACTION_ID_TINY_KEY = @"tid";
        public const string WALLET_REGISTER_ID_TINY_KEY = @"rid";
        public const string WALLET_TRANSACTION_TOTAL_AMOUNT_TINY_KEY = @"tamt";
        public const string WALLET_TRANSACTION_REMAINING_AMOUNT_TINY_KEY = @"ramt";
        public const string WALLET_AMOUNT_TINY_KEY = @"amt";
        public const string WALLET_DEVICE_ID_TINY_KEY = @"did";
        public const string WALLET_ERRORS_TINY_KEY = @"errs";
    }
}
