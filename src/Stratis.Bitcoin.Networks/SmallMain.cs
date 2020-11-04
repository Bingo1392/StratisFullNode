using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class SmallMain : Network
    {
        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Stratis default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Stratis blockchains (StratisMain, StratisTest, StratisRegTest). </summary>
        public const string SmallRootFolderName = "small";

        /// <summary> The default name used for the Stratis configuration file. </summary>
        public const string SmallDefaultConfigFilename = "small.conf";
        
        public SmallMain()
        {
            this.Name = "SmallMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("Small"));
            this.DefaultPort = 18555;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 100;
            this.DefaultRPCPort = 18554;
            this.DefaultAPIPort = 18553;
            this.DefaultSignalRPort = 18552;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = SmallRootFolderName;
            this.DefaultConfigFilename = SmallDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "SMALL";
            this.DefaultBanTimeSeconds = 11250;
            
            // To successfully process the OP_FEDERATION opcode the federations should be known.
            this.Federations = new Federations();
            this.Federations.RegisterFederation(new Federation(new[]
            {
                new PubKey("03f5de5176e29e1e7d518ae76c1e020b1da18b57a3713ac81b16015026e232748e")
            }));
            
            var consensusFactory = new PosConsensusFactory();
            
            // Create the genesis block.
            this.GenesisTime = 1603128356; // 19 October 2020
            this.GenesisNonce = 1831492;
            this.GenesisBits = 0x1e0fffff; // The difficulty target
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;
            
            // TODO: Update the genesis text to a dated newspaper article closer to launch
            Block genesisBlock = StraxNetwork.CreateGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward, "smallgenesisblock");

            this.Genesis = genesisBlock;
            
            // Taken from Stratis.
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5,
                witnessScaleFactor: 4
            );
            
            
            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };
            
            var bip9Deployments = new StraxBIP9Deployments()
            {
                // Always active.
                [StraxBIP9Deployments.CSV] = new BIP9DeploymentsParameters("CSV", 0, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [StraxBIP9Deployments.Segwit] = new BIP9DeploymentsParameters("Segwit", 1, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [StraxBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold)
            };
            
            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 1, // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: null,
                minerConfirmationWindow: 2016,
                maxReorgLength: 500,
                defaultAssumeValid: null, // TODO: Set this once some checkpoint candidates have elapsed
                maxMoney: long.MaxValue,
                coinbaseMaturity: 5,
                premineHeight: 2,
                premineReward: Money.Coins(125000000),
                proofOfWorkReward: Money.Coins(18),
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60),
                targetSpacing: TimeSpan.FromSeconds(45),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 10,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(18)
            );
            
            this.Consensus.PosEmptyCoinbase = false;
            
            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { 75 }; // X
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 140 }; // y
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (75 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };
            
            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
            };
            
            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder("small");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;
            
            this.DNSSeeds = new List<DNSSeedData>
            {
                // new DNSSeedData("mainnet1.stratisnetwork.com", "mainnet1.stratisnetwork.com")
            };

            this.SeedNodes = new List<NetworkAddress>
            {
                new NetworkAddress(IPAddress.Parse("127.0.0.1"), 18555), // node-1
                new NetworkAddress(IPAddress.Parse("127.0.0.1"), 19555), // node-2
                new NetworkAddress(IPAddress.Parse("127.0.0.1"), 20555), // node-3
            };
            
            this.StandardScriptsRegistry = new StraxStandardScriptsRegistry();
            
            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * this.Consensus.TargetSpacing.TotalSeconds / 2);

            // TODO: Update these when the final block is mined
            // TODO: This consensus hashGenesisBlock isn't the same as I wrote here. I have to change it.
            //Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x00000921702bd55eb8c4318a8dbcfca29b9d340b1856c6af0b8962a3a0e12fff"));
            //Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0xb21368a732cb9ae9b34a45eea13ce1b7cdb3c4b02991d3f715022d67d2b51c8d"));

            StraxNetwork.RegisterRules(this.Consensus);
            StraxNetwork.RegisterMempoolRules(this.Consensus);
        }
    }
}