﻿using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Interfaces;
using Stratis.Bitcoin.PoA.Features.Voting;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.PoA.Voting
{
    public static class JoinFederationRequestBuilder
    {
        public const decimal VotingRequestTransferAmount = 500m;
        public const int VotingRequestExpectedInputCount = 1;
        public const int VotingRequestExpectedOutputCount = 2;

        public static Transaction BuildTransaction(IWalletTransactionHandler walletTransactionHandler, Network network, JoinFederationRequest request, JoinFederationRequestEncoder encoder, string walletName, string walletAccount, string walletPassword)
        {
            byte[] encodedVotingRequest = encoder.Encode(request);
            var votingOutputScript = new Script(OpcodeType.OP_RETURN, Op.GetPushOp(encodedVotingRequest));

            var context = new TransactionBuildContext(network)
            {
                AccountReference = new WalletAccountReference(walletName, walletAccount),
                MinConfirmations = 0,
                FeeType = FeeType.High,
                WalletPassword = walletPassword,
                Recipients = new[] { new Recipient { Amount = new Money(VotingRequestTransferAmount, MoneyUnit.BTC), ScriptPubKey = votingOutputScript } }.ToList()
            };

            Transaction trx = walletTransactionHandler.BuildTransaction(context);

            Guard.Assert(IsVotingRequestTransaction(trx, encoder));
            Guard.Assert(context.TransactionBuilder.Verify(trx, out _));

            return trx;
        }

        public static JoinFederationRequest Deconstruct(Transaction trx, JoinFederationRequestEncoder encoder)
        {
            if (trx.Inputs.Count != VotingRequestExpectedInputCount)
                return null;

            if (trx.Outputs.Count != VotingRequestExpectedOutputCount)
                return null;

            IList<Op> ops = trx.Outputs[1].ScriptPubKey.ToOps();

            if (ops[0].Code != OpcodeType.OP_RETURN)
                return null;

            if (trx.Outputs[1].Value.ToDecimal(MoneyUnit.BTC) != VotingRequestTransferAmount)
                return null;

            return encoder.Decode(ops[1].PushData);
        }

        public static bool IsVotingRequestTransaction(Transaction trx, JoinFederationRequestEncoder encoder)
        {
            return Deconstruct(trx, encoder) != null;
        }
    }
}
