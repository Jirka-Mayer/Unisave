using System.Collections;
using System.Text.RegularExpressions;
using LightJson;
using NUnit.Framework;
using Steamworks;
using Unisave.Facades;
using Unisave.Testing;
using UnisaveFixture.SteamMicrotransactions;
using UnityEngine;
using UnityEngine.TestTools;

// Steam documentation of the entire process:
// https://partner.steamgames.com/doc/features/microtransactions/implementation

namespace UnisaveFixture.Tests.SteamMicrotransactions
{
    [TestFixture]
    public class SteamMicrotransactionTest : BackendTestCase
    {
        private class SteamPurchasingClientMock
            : SteamPurchasingClient
        {
            protected override ulong GetSteamId()
            {
                return 123456789L;
            }
        }
        
        public override void SetUp()
        {
            base.SetUp();
            
            // fail-load the steam manager
            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    @"\[Steamworks\.NET\] SteamAPI_Init\(\) failed\."
                )
            );
            Assert.IsFalse(SteamManager.Initialized);
        }

        [UnityTest]
        public IEnumerator ClickingBuyInitiatesTransaction()
        {
            // setup scene
            var go = new GameObject();
            var smm = go.AddComponent<SteamPurchasingClientMock>();
            yield return null;

            // setup HTTP
            Http.Fake(Http.Response(new JsonObject {
                ["response"] = new JsonObject {
                    ["result"] = "OK",
                    ["params"] = new JsonObject {
                        ["orderid"] = "938473",
                        ["transid"] = "374839"
                    }
                }
            }, 200));
            
            // click the button
            smm.PlayerClickedBuy();

            // check the
            //var requests = Http.Recorded();
            
            // assert request to initiate transaction has been sent
            Http.AssertSent(request =>
                request.Url == "https://steam-stuff.com/" &&
                request.HasHeader("Accept", "application/json") &&
                request["key"] == "..." &&
                request["appid"] == "..."
            );
//            Http.AssertNothingSent();
            
            // now the Steamworks API will notify the game via a callback
        }

        [UnityTest]
        public IEnumerator ReceivingPositiveUserResponseFinishesTransaction()
        {
            // scene setup
            var go = new GameObject();
            var smm = go.AddComponent<SteamPurchasingClient>();
            
            yield return null;
            
            smm.AfterSteamHandledCheckout(
                new MicroTxnAuthorizationResponse_t {
                    m_ulOrderID = 42,
                    m_unAppID = 440,
                    m_bAuthorized = 1
                }
            );
            
            // assert finalization happens
        }
    }
}