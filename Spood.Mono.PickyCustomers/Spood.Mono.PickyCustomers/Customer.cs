//// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
//// ScheduleOne.Economy.Customer
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using EasyButtons;
//using FishNet;
//using FishNet.Connection;
//using FishNet.Managing;
//using FishNet.Object;
//using FishNet.Object.Synchronizing;
//using FishNet.Serializing;
//using FishNet.Serializing.Generated;
//using FishNet.Transporting;
//using ScheduleOne;
//using ScheduleOne.DevUtilities;
//using ScheduleOne.Dialogue;
//using ScheduleOne.Economy;
//using ScheduleOne.GameTime;
//using ScheduleOne.ItemFramework;
//using ScheduleOne.Law;
//using ScheduleOne.Levelling;
//using ScheduleOne.Map;
//using ScheduleOne.Messaging;
//using ScheduleOne.Money;
//using ScheduleOne.NPCs;
//using ScheduleOne.NPCs.Relation;
//using ScheduleOne.NPCs.Responses;
//using ScheduleOne.NPCs.Schedules;
//using ScheduleOne.Persistence;
//using ScheduleOne.Persistence.Datas;
//using ScheduleOne.Persistence.Loaders;
//using ScheduleOne.PlayerScripts;
//using ScheduleOne.Product;
//using ScheduleOne.Properties;
//using ScheduleOne.Quests;
//using ScheduleOne.UI;
//using ScheduleOne.UI.Handover;
//using ScheduleOne.UI.Phone.Messages;
//using ScheduleOne.Variables;
//using ScheduleOne.VoiceOver;
//using UnityEngine;
//using UnityEngine.Events;

//[DisallowMultipleComponent]
//[RequireComponent(typeof(NPC))]
//public class Customer : NetworkBehaviour, ISaveable
//{
//    [Serializable]
//    public class ScheduleGroupPair
//    {
//        public GameObject NormalScheduleGroup;

//        public GameObject CurfewScheduleGroup;
//    }

//    [Serializable]
//    public class CustomerPreference
//    {
//        public EDrugType DrugType;

//        [Header("Optionally, a specific product")]
//        public ProductDefinition Definition;

//        public EQuality MinimumQuality;
//    }

//    public enum ESampleFeedback
//    {
//        WrongProduct,
//        WrongQuality,
//        Correct
//    }

//    public static Action<Customer> onCustomerUnlocked;

//    public static List<Customer> UnlockedCustomers = new List<Customer>();

//    public const float AFFINITY_MAX_EFFECT = 0.3f;

//    public const float PROPERTY_MAX_EFFECT = 0.4f;

//    public const float QUALITY_MAX_EFFECT = 0.3f;

//    public const float DEAL_REJECTED_RELATIONSHIP_CHANGE = -0.5f;

//    public bool DEBUG;

//    public const float APPROACH_MIN_ADDICTION = 0.33f;

//    public const float APPROACH_CHANCE_PER_DAY_MAX = 0.5f;

//    public const float APPROACH_MIN_COOLDOWN = 2160f;

//    public const float APPROACH_MAX_COOLDOWN = 4320f;

//    public const int DEAL_COOLDOWN = 600;

//    public static string[] PlayerAcceptMessages = new string[5] { "Yes", "Sure thing", "Yep", "Deal", "Alright" };

//    public static string[] PlayerRejectMessages = new string[3] { "No", "Not right now", "No, sorry" };

//    public const int DEAL_ATTENDANCE_TOLERANCE = 10;

//    public const int MIN_TRAVEL_TIME = 15;

//    public const int MAX_TRAVEL_TIME = 360;

//    public const int OFFER_EXPIRY_TIME_MINS = 600;

//    public const float MIN_ORDER_APPEAL = 0.05f;

//    public const float ADDICTION_DRAIN_PER_DAY = 0.0625f;

//    public const bool SAMPLE_REQUIRES_RECOMMENDATION = false;

//    public const float MIN_NORMALIZED_RELATIONSHIP_FOR_RECOMMENDATION = 0.5f;

//    public const float RELATIONSHIP_FOR_GUARANTEED_DEALER_RECOMMENDATION = 0.6f;

//    public const float RELATIONSHIP_FOR_GUARANTEED_SUPPLIER_RECOMMENDATION = 0.6f;

//    [CompilerGenerated]
//    [SyncVar]
//    public float <CurrentAddiction>k__BackingField;

//	private ContractInfo offeredContractInfo;

//    [CompilerGenerated]
//    [SyncVar]
//    public bool <HasBeenRecommended>k__BackingField;

//	public NPCSignal_WaitForDelivery DealSignal;

//    [Header("Settings")]
//    public bool AvailableInDemo = true;

//    [SerializeField]
//    protected ScheduleOne.Economy.CustomerData customerData;

//    public DeliveryLocation DefaultDeliveryLocation;

//    public bool CanRecommendFriends = true;

//    [Header("Events")]
//    public UnityEvent onUnlocked;

//    public UnityEvent onDealCompleted;

//    public UnityEvent<Contract> onContractAssigned;

//    private bool awaitingSample;

//    private DialogueController.DialogueChoice sampleChoice;

//    private DialogueController.DialogueChoice completeContractChoice;

//    private DialogueController.DialogueChoice offerDealChoice;

//    private DialogueController.GreetingOverride awaitingDealGreeting;

//    private int minsSinceUnlocked = 10000;

//    private bool sampleOfferedToday;

//    private CustomerAffinityData currentAffinityData;

//    private bool pendingInstantDeal;

//    private ProductItemInstance consumedSample;

//    public SyncVar<float> syncVar___<CurrentAddiction>k__BackingField;

//    public SyncVar<bool> syncVar___<HasBeenRecommended>k__BackingField;

//    private bool NetworkInitialize___EarlyScheduleOne.Economy.CustomerAssembly-CSharp.dll_Excuted;

//    private bool NetworkInitialize__LateScheduleOne.Economy.CustomerAssembly-CSharp.dll_Excuted;

//    public float CurrentAddiction
//    {
//        [CompilerGenerated]
//        get
//        {
//            return SyncAccessor_ < CurrentAddiction > k__BackingField;
//        }
//        [CompilerGenerated]
//        protected set
//        {
//            this.sync___set_value_ < CurrentAddiction > k__BackingField(value, asServer: true);
//        }
//    }

//    public ContractInfo OfferedContractInfo
//    {
//        get
//        {
//            return offeredContractInfo;
//        }
//        protected set
//        {
//            offeredContractInfo = value;
//        }
//    }

//    public GameDateTime OfferedContractTime { get; protected set; }

//    public Contract CurrentContract { get; protected set; }

//    public bool IsAwaitingDelivery { get; protected set; }

//    public int TimeSinceLastDealCompleted { get; protected set; } = 1000000;

//    public int TimeSinceLastDealOffered { get; protected set; } = 1000000;

//    public int TimeSincePlayerApproached { get; protected set; } = 1000000;

//    public int TimeSinceInstantDealOffered { get; protected set; } = 1000000;

//    public int OfferedDeals { get; protected set; }

//    public int CompletedDeliveries { get; protected set; }

//    public bool HasBeenRecommended
//    {
//        [CompilerGenerated]
//        get
//        {
//            return SyncAccessor_ < HasBeenRecommended > k__BackingField;
//        }
//        [CompilerGenerated]
//        protected set
//        {
//            this.sync___set_value_ < HasBeenRecommended > k__BackingField(value, asServer: true);
//        }
//    }

//    public NPC NPC { get; protected set; }

//    public Dealer AssignedDealer { get; protected set; }

//    public ScheduleOne.Economy.CustomerData CustomerData => customerData;

//    public List<ProductDefinition> OrderableProducts
//    {
//        get
//        {
//            if (!(AssignedDealer != null))
//            {
//                return ProductManager.ListedProducts;
//            }
//            return AssignedDealer.GetOrderableProducts();
//        }
//    }

//    private DialogueDatabase dialogueDatabase => NPC.dialogueHandler.Database;

//    public NPCPoI potentialCustomerPoI { get; private set; }

//    public string SaveFolderName => "CustomerData";

//    public string SaveFileName => "CustomerData";

//    public Loader Loader => null;

//    public bool ShouldSaveUnderFolder => false;

//    public List<string> LocalExtraFiles { get; set; } = new List<string>();

//    public List<string> LocalExtraFolders { get; set; } = new List<string>();

//    public bool HasChanged { get; set; }

//    public float SyncAccessor_<CurrentAddiction>k__BackingField

//    {
//        get

//        {
//            return CurrentAddiction;
//        }
//        set

//        {
//            if (value || !base.IsServerInitialized)
//            {
//                CurrentAddiction = value;
//            }
//            if (Application.isPlaying)
//            {
//                syncVar___<CurrentAddiction> k__BackingField.SetValue(value, value);
//            }
//        }
//    }

//    public bool SyncAccessor_<HasBeenRecommended>k__BackingField

//    {
//        get

//        {
//            return HasBeenRecommended;
//        }
//        set

//        {
//            if (value || !base.IsServerInitialized)
//            {
//                HasBeenRecommended = value;
//            }
//            if (Application.isPlaying)
//            {
//                syncVar___<HasBeenRecommended> k__BackingField.SetValue(value, value);
//            }
//        }
//    }

//    public virtual void Awake()
//    {
//        NetworkInitialize___Early();
//        Awake_UserLogic_ScheduleOne.Economy.Customer_Assembly - CSharp.dll();
//        NetworkInitialize__Late();
//    }

//    protected override void OnValidate()
//    {
//        base.OnValidate();
//        if (DealSignal == null)
//        {
//            NPCAction nPCAction = GetComponentInChildren<NPCScheduleManager>().ActionList.Find((NPCAction x) => x != null && x.GetType() == typeof(NPCSignal_WaitForDelivery));
//            if (nPCAction == null)
//            {
//                GameObject obj = new GameObject("DealSignal");
//                obj.transform.SetParent(GetComponentInChildren<NPCScheduleManager>().transform);
//                nPCAction = obj.AddComponent<NPCSignal_WaitForDelivery>();
//            }
//            DealSignal = nPCAction as NPCSignal_WaitForDelivery;
//        }
//        if (DealSignal != null)
//        {
//            DealSignal.gameObject.SetActive(value: false);
//        }
//    }

//    private void Start()
//    {
//        TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
//        instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
//        TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
//        instance2.onDayPass = (Action)Delegate.Combine(instance2.onDayPass, new Action(DayPass));
//        if (NPC.RelationData.Unlocked)
//        {
//            OnCustomerUnlocked(NPCRelationData.EUnlockType.DirectApproach, notify: false);
//        }
//        else
//        {
//            NPCRelationData relationData = NPC.RelationData;
//            relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(OnCustomerUnlocked));
//        }
//        foreach (NPC connection in NPC.RelationData.Connections)
//        {
//            if (!(connection == null))
//            {
//                NPCRelationData relationData2 = connection.RelationData;
//                relationData2.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData2.onUnlocked, (Action<NPCRelationData.EUnlockType, bool>)delegate
//                {
//                    UpdatePotentialCustomerPoI();
//                });
//            }
//        }
//        if (NPC.MSGConversation != null)
//        {
//            RegisterLoadEvent();
//        }
//        else
//        {
//            NPC nPC = NPC;
//            nPC.onConversationCreated = (Action)Delegate.Combine(nPC.onConversationCreated, new Action(RegisterLoadEvent));
//        }
//        SetUpDialogue();
//        void RegisterLoadEvent()
//        {
//            SetUpResponseCallbacks();
//            MSGConversation mSGConversation = NPC.MSGConversation;
//            mSGConversation.onLoaded = (Action)Delegate.Combine(mSGConversation.onLoaded, new Action(SetUpResponseCallbacks));
//            MSGConversation mSGConversation2 = NPC.MSGConversation;
//            mSGConversation2.onResponsesShown = (Action)Delegate.Combine(mSGConversation2.onResponsesShown, new Action(SetUpResponseCallbacks));
//        }
//    }

//    public override void OnStartClient()
//    {
//        base.OnStartClient();
//        SetupPoI();
//    }

//    public override void OnSpawnServer(NetworkConnection connection)
//    {
//        base.OnSpawnServer(connection);
//        if (!connection.IsLocalClient)
//        {
//            ReceiveCustomerData(connection, GetCustomerData());
//            if (DealSignal.IsActive)
//            {
//                ConfigureDealSignal(connection, DealSignal.StartTime, active: true);
//            }
//        }
//    }

//    private void OnDestroy()
//    {
//        UnlockedCustomers.Remove(this);
//    }

//    private void SetUpDialogue()
//    {
//        sampleChoice = new DialogueController.DialogueChoice();
//        sampleChoice.ChoiceText = "Can I interest you in a free sample?";
//        sampleChoice.Enabled = true;
//        sampleChoice.Conversation = null;
//        sampleChoice.onChoosen = new UnityEvent();
//        sampleChoice.onChoosen.AddListener(SampleOffered);
//        sampleChoice.shouldShowCheck = ShowDirectApproachOption;
//        sampleChoice.isValidCheck = SampleOptionValid;
//        NPC.dialogueHandler.GetComponent<DialogueController>().AddDialogueChoice(sampleChoice, -20);
//        completeContractChoice = new DialogueController.DialogueChoice();
//        completeContractChoice.ChoiceText = "[Complete Deal]";
//        completeContractChoice.Enabled = true;
//        completeContractChoice.Conversation = null;
//        completeContractChoice.onChoosen = new UnityEvent();
//        completeContractChoice.onChoosen.AddListener(HandoverChosen);
//        completeContractChoice.shouldShowCheck = IsReadyForHandover;
//        completeContractChoice.isValidCheck = IsHandoverChoiceValid;
//        NPC.dialogueHandler.GetComponent<DialogueController>().AddDialogueChoice(completeContractChoice, 10);
//        offerDealChoice = new DialogueController.DialogueChoice();
//        offerDealChoice.ChoiceText = "You wanna buy something? [Offer a deal]";
//        offerDealChoice.Enabled = true;
//        offerDealChoice.Conversation = null;
//        offerDealChoice.onChoosen = new UnityEvent();
//        offerDealChoice.onChoosen.AddListener(InstantDealOffered);
//        offerDealChoice.shouldShowCheck = ShowOfferDealOption;
//        offerDealChoice.isValidCheck = OfferDealValid;
//        NPC.dialogueHandler.GetComponent<DialogueController>().AddDialogueChoice(offerDealChoice);
//        awaitingDealGreeting = new DialogueController.GreetingOverride();
//        awaitingDealGreeting.Greeting = dialogueDatabase.GetLine(EDialogueModule.Customer, "awaiting_deal");
//        awaitingDealGreeting.ShouldShow = false;
//        awaitingDealGreeting.PlayVO = true;
//        awaitingDealGreeting.VOType = EVOLineType.Question;
//        NPC.dialogueHandler.GetComponent<DialogueController>().AddGreetingOverride(awaitingDealGreeting);
//    }

//    private void SetupPoI()
//    {
//        if (!(potentialCustomerPoI != null))
//        {
//            potentialCustomerPoI = UnityEngine.Object.Instantiate(NetworkSingleton<NPCManager>.Instance.PotentialCustomerPoIPrefab, base.transform);
//            potentialCustomerPoI.SetMainText("Potential Customer\n" + NPC.fullName);
//            potentialCustomerPoI.SetNPC(NPC);
//            float y = (float)(NPC.FirstName[0] % 36) * 10f;
//            float num = Mathf.Clamp((float)NPC.FirstName.Length * 1.5f, 1f, 10f);
//            Vector3 forward = base.transform.forward;
//            forward = Quaternion.Euler(0f, y, 0f) * forward;
//            potentialCustomerPoI.transform.localPosition = forward * num;
//            UpdatePotentialCustomerPoI();
//        }
//    }

//    public virtual void InitializeSaveable()
//    {
//        Singleton<SaveManager>.Instance.RegisterSaveable(this);
//    }

//    protected virtual void MinPass()
//    {
//        TimeSincePlayerApproached++;
//        TimeSinceLastDealCompleted++;
//        TimeSinceLastDealOffered++;
//        minsSinceUnlocked++;
//        TimeSinceInstantDealOffered++;
//        if (!InstanceFinder.IsServer)
//        {
//            return;
//        }
//        HasChanged = true;
//        if (DEBUG)
//        {
//            ScheduleOne.Console.Log("Current contract: " + CurrentContract);
//            ScheduleOne.Console.Log("Offered contract: " + OfferedContractInfo);
//            ScheduleOne.Console.Log("Awaiting sample: " + awaitingSample);
//            ScheduleOne.Console.Log("Sample offered today: " + sampleOfferedToday);
//            ScheduleOne.Console.Log("Dealer: " + AssignedDealer);
//            ScheduleOne.Console.Log("Awaiting deal: " + IsAwaitingDelivery);
//        }
//        if (ShouldTryGenerateDeal())
//        {
//            ContractInfo contractInfo = CheckContractGeneration();
//            if (contractInfo != null)
//            {
//                if (AssignedDealer != null)
//                {
//                    if (AssignedDealer.ShouldAcceptContract(contractInfo, this))
//                    {
//                        OfferedDeals++;
//                        TimeSinceLastDealOffered = 0;
//                        OfferedContractInfo = contractInfo;
//                        OfferedContractTime = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime();
//                        HasChanged = true;
//                        AssignedDealer.ContractedOffered(contractInfo, this);
//                    }
//                }
//                else
//                {
//                    OfferContract(contractInfo);
//                }
//            }
//        }
//        if (ShouldTryApproachPlayer())
//        {
//            float num = Mathf.Lerp(0f, 0.5f, SyncAccessor_ < CurrentAddiction > k__BackingField);
//            if (UnityEngine.Random.Range(0f, 1f) < num / 1440f)
//            {
//                Player randomPlayer = Player.GetRandomPlayer();
//                ScheduleOne.Console.Log("Approaching player: " + randomPlayer);
//                if (randomPlayer != null)
//                {
//                    RequestProduct(randomPlayer);
//                }
//            }
//        }
//        if (OfferedContractInfo != null)
//        {
//            UpdateOfferExpiry();
//        }
//        else
//        {
//            NPC.MSGConversation?.SetSliderValue(0f, Color.white);
//        }
//        if (CurrentContract != null)
//        {
//            UpdateDealAttendance();
//        }
//    }

//    protected virtual void DayPass()
//    {
//        sampleOfferedToday = false;
//        if (InstanceFinder.IsServer && (float)TimeSinceLastDealCompleted / 60f >= 24f)
//        {
//            ChangeAddiction(-0.0625f);
//        }
//    }

//    private void UpdateDealAttendance()
//    {
//        if (CurrentContract == null)
//        {
//            return;
//        }
//        float num = Vector3.Distance(NPC.Avatar.CenterPoint, CurrentContract.DeliveryLocation.CustomerStandPoint.position);
//        if (DEBUG)
//        {
//            ScheduleOne.Console.Log("1");
//        }
//        if (!NPC.IsConscious)
//        {
//            CurrentContract.Fail();
//            return;
//        }
//        if (DEBUG)
//        {
//            ScheduleOne.Console.Log("2");
//        }
//        if (DealSignal.IsActive && IsAwaitingDelivery && num < 10f)
//        {
//            return;
//        }
//        int windowStartTime = CurrentContract.DeliveryWindow.WindowStartTime;
//        int num2 = ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(CurrentContract.DeliveryWindow.WindowStartTime, 10);
//        int windowEndTime = CurrentContract.DeliveryWindow.WindowEndTime;
//        if (DEBUG)
//        {
//            ScheduleOne.Console.Log("Soft start: " + windowStartTime);
//            ScheduleOne.Console.Log("Hard start: " + num2);
//            ScheduleOne.Console.Log("End time: " + windowEndTime);
//        }
//        if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(num2, windowEndTime))
//        {
//            if (!DealSignal.IsActive)
//            {
//                ConfigureDealSignal(null, NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime, active: true);
//            }
//            if (num > Vector3.Distance(CurrentContract.DeliveryLocation.TeleportPoint.position, CurrentContract.DeliveryLocation.CustomerStandPoint.position) * 2f)
//            {
//                NPC.Movement.Warp(CurrentContract.DeliveryLocation.TeleportPoint.position);
//            }
//        }
//        else if (!DealSignal.IsActive)
//        {
//            int value = Mathf.CeilToInt(num / NPC.Movement.WalkSpeed * 2f);
//            value = Mathf.Clamp(value, 15, 360);
//            int min = ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(windowStartTime, -(value + 10));
//            if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(min, num2))
//            {
//                ConfigureDealSignal(null, NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime, active: true);
//            }
//        }
//    }

//    [ObserversRpc(RunLocally = true)]
//    [TargetRpc]
//    private void ConfigureDealSignal(NetworkConnection conn, int startTime, bool active)
//    {
//        if ((object)conn == null)
//        {
//            RpcWriter___Observers_ConfigureDealSignal_338960014(conn, startTime, active);
//            RpcLogic___ConfigureDealSignal_338960014(conn, startTime, active);
//        }
//        else
//        {
//            RpcWriter___Target_ConfigureDealSignal_338960014(conn, startTime, active);
//        }
//    }

//    private void UpdateOfferExpiry()
//    {
//        if (!InstanceFinder.IsServer || GameManager.IS_TUTORIAL)
//        {
//            return;
//        }
//        if (OfferedContractInfo == null)
//        {
//            NPC.MSGConversation.SetSliderValue(0f, Color.white);
//            return;
//        }
//        int num = OfferedContractTime.GetMinSum() + 600;
//        int minSum = OfferedContractTime.GetMinSum();
//        float num2 = Mathf.Clamp01((float)(NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetTotalMinSum() - minSum) / 600f);
//        NPC.MSGConversation.SetSliderValue(1f - num2, Singleton<HUD>.Instance.RedGreenGradient.Evaluate(1f - num2));
//        if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetTotalMinSum() > num)
//        {
//            ExpireOffer();
//            OfferedContractInfo = null;
//        }
//    }

//    private ContractInfo CheckContractGeneration(bool force = false)
//    {
//        if (!ShouldTryGenerateDeal() && !force)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " should not try to generate a deal");
//            }
//            return null;
//        }
//        if (OrderableProducts.Count == 0)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " has no orderable products");
//            }
//            return null;
//        }
//        if (AssignedDealer == null)
//        {
//            if (!ProductManager.IsAcceptingOrders && !force)
//            {
//                if (DEBUG)
//                {
//                    ScheduleOne.Console.LogWarning("Not accepting orders");
//                }
//                return null;
//            }
//            if (NetworkSingleton<ProductManager>.Instance.TimeSinceProductListingChanged < 3f && !force)
//            {
//                if (DEBUG)
//                {
//                    ScheduleOne.Console.LogWarning("Product listing changed too recently");
//                }
//                return null;
//            }
//        }
//        int num = 7;
//        if (AssignedDealer == null)
//        {
//            List<EDay> orderDays = customerData.GetOrderDays(SyncAccessor_ < CurrentAddiction > k__BackingField, NPC.RelationData.RelationDelta / 5f);
//            num = orderDays.Count;
//            if (!orderDays.Contains(NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentDay) && !force)
//            {
//                if (DEBUG)
//                {
//                    ScheduleOne.Console.LogWarning(NPC.fullName + " cannot order today");
//                }
//                return null;
//            }
//        }
//        int orderTime = customerData.OrderTime;
//        int num2 = 0;
//        num2 = ((!(AssignedDealer == null)) ? ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(orderTime, 360) : ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(orderTime, 120));
//        if (!NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(orderTime, num2) && !force)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning(NPC.fullName + " cannot order now");
//            }
//            return null;
//        }
//        float num3 = customerData.GetAdjustedWeeklySpend(NPC.RelationData.RelationDelta / 5f) / (float)num;
//        float appeal;
//        ProductDefinition weightedRandomProduct = GetWeightedRandomProduct(out appeal);
//        if (weightedRandomProduct == null || appeal < 0.05f)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.Log(NPC.fullName + " has too low appeal for any products");
//            }
//            return null;
//        }
//        EQuality correspondingQuality = customerData.Standards.GetCorrespondingQuality();
//        float productEnjoyment = GetProductEnjoyment(weightedRandomProduct, correspondingQuality);
//        float num4 = weightedRandomProduct.Price * Mathf.Lerp(0.66f, 1.5f, productEnjoyment);
//        num3 *= Mathf.Lerp(0.66f, 1.5f, productEnjoyment);
//        int value = Mathf.RoundToInt(num3 / weightedRandomProduct.Price);
//        value = Mathf.Clamp(value, 1, 1000);
//        if (AssignedDealer != null)
//        {
//            int productCount = AssignedDealer.GetProductCount(weightedRandomProduct.ID, correspondingQuality, EQuality.Heavenly);
//            if (productCount < value)
//            {
//                value = productCount;
//            }
//        }
//        if (value >= 14)
//        {
//            value = Mathf.RoundToInt(value / 5) * 5;
//        }
//        float payment = Mathf.RoundToInt(num4 * (float)value / 5f) * 5;
//        ProductList productList = new ProductList();
//        productList.entries.Add(new ProductList.Entry
//        {
//            ProductID = weightedRandomProduct.ID,
//            Quantity = value,
//            Quality = correspondingQuality
//        });
//        QuestWindowConfig deliveryWindow = new QuestWindowConfig
//        {
//            IsEnabled = true,
//            WindowStartTime = 0,
//            WindowEndTime = 0
//        };
//        DeliveryLocation deliveryLocation = DefaultDeliveryLocation;
//        if (!GameManager.IS_TUTORIAL)
//        {
//            deliveryLocation = Singleton<Map>.Instance.GetRegionData(NPC.Region).GetRandomUnscheduledDeliveryLocation();
//            if (deliveryLocation == null)
//            {
//                ScheduleOne.Console.LogError("No unscheduled delivery locations found for " + NPC.Region);
//                return null;
//            }
//        }
//        return new ContractInfo(payment, productList, deliveryLocation.GUID.ToString(), deliveryWindow, expires: true, 1, 0, isCounterOffer: false);
//    }

//    private ProductDefinition GetWeightedRandomProduct(out float appeal)
//    {
//        float num = UnityEngine.Random.Range(0f, 1f);
//        Dictionary<ProductDefinition, float> productAppeal = new Dictionary<ProductDefinition, float>();
//        for (int i = 0; i < OrderableProducts.Count; i++)
//        {
//            float productEnjoyment = GetProductEnjoyment(OrderableProducts[i], customerData.Standards.GetCorrespondingQuality());
//            float num2 = OrderableProducts[i].Price / OrderableProducts[i].MarketValue;
//            float num3 = Mathf.Lerp(1f, -1f, num2 / 2f);
//            float value = productEnjoyment + num3;
//            productAppeal.Add(OrderableProducts[i], value);
//        }
//        OrderableProducts.OrderByDescending((ProductDefinition x) => productAppeal[x]).ToList();
//        if (num <= 0.5f || OrderableProducts.Count <= 1)
//        {
//            appeal = productAppeal[OrderableProducts[0]];
//            return OrderableProducts[0];
//        }
//        if (num <= 0.75f || OrderableProducts.Count <= 2)
//        {
//            appeal = productAppeal[OrderableProducts[1]];
//            return OrderableProducts[1];
//        }
//        if (num <= 0.875f || OrderableProducts.Count <= 3)
//        {
//            appeal = productAppeal[OrderableProducts[2]];
//            return OrderableProducts[2];
//        }
//        appeal = productAppeal[OrderableProducts[3]];
//        return OrderableProducts[3];
//    }

//    protected virtual void OnCustomerUnlocked(NPCRelationData.EUnlockType unlockType, bool notify)
//    {
//        if (notify)
//        {
//            Singleton<NewCustomerPopup>.Instance.PlayPopup(this);
//            minsSinceUnlocked = 0;
//        }
//        UnlockedCustomers.Add(this);
//        if (onUnlocked != null)
//        {
//            onUnlocked.Invoke();
//        }
//        if (onCustomerUnlocked != null)
//        {
//            onCustomerUnlocked(this);
//        }
//        UpdatePotentialCustomerPoI();
//    }

//    public void SetHasBeenRecommended()
//    {
//        HasBeenRecommended = true;
//        HasChanged = true;
//    }

//    public virtual void OfferContract(ContractInfo info)
//    {
//        DialogueChain chain = NPC.dialogueHandler.Database.GetChain(EDialogueModule.Customer, "contract_request");
//        if (OfferedDeals == 0 && NPC.dialogueHandler.Database.HasChain(EDialogueModule.Generic, "first_contract_request"))
//        {
//            chain = NPC.dialogueHandler.Database.GetChain(EDialogueModule.Generic, "first_contract_request");
//        }
//        chain = info.ProcessMessage(chain);
//        NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Offered_Contract_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Offered_Contract_Count") + 1f).ToString());
//        OfferedDeals++;
//        TimeSinceLastDealOffered = 0;
//        OfferedContractInfo = info;
//        OfferedContractTime = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime();
//        NotifyPlayerOfContract(OfferedContractInfo, chain.GetMessageChain(), canAccept: true, canReject: true);
//        HasChanged = true;
//        SetOfferedContract(OfferedContractInfo, OfferedContractTime);
//    }

//    [ObserversRpc]
//    private void SetOfferedContract(ContractInfo info, GameDateTime offerTime)
//    {
//        RpcWriter___Observers_SetOfferedContract_4277245194(info, offerTime);
//    }

//    [ServerRpc(RequireOwnership = false, RunLocally = true)]
//    public virtual void ExpireOffer()
//    {
//        RpcWriter___Server_ExpireOffer_2166136261();
//        RpcLogic___ExpireOffer_2166136261();
//    }

//    public virtual void AssignContract(Contract contract)
//    {
//        CurrentContract = contract;
//        CurrentContract.onQuestEnd.AddListener(CurrentContractEnded);
//        DealSignal.Location = CurrentContract.DeliveryLocation;
//        if (onContractAssigned != null)
//        {
//            onContractAssigned.Invoke(contract);
//        }
//    }

//    protected virtual void NotifyPlayerOfContract(ContractInfo contract, MessageChain offerMessage, bool canAccept, bool canReject, bool canCounterOffer = true)
//    {
//        NPC.MSGConversation.SendMessageChain(offerMessage);
//        List<Response> list = new List<Response>();
//        if (canAccept)
//        {
//            list.Add(new Response(PlayerAcceptMessages[UnityEngine.Random.Range(0, PlayerAcceptMessages.Length - 1)], "ACCEPT_CONTRACT", AcceptContractClicked, _disableDefaultResponseBehaviour: true));
//        }
//        if (canCounterOffer)
//        {
//            list.Add(new Response("[Counter-offer]", "COUNTEROFFER", CounterOfferClicked, _disableDefaultResponseBehaviour: true));
//        }
//        if (canReject)
//        {
//            list.Add(new Response(PlayerRejectMessages[UnityEngine.Random.Range(0, PlayerRejectMessages.Length - 1)], "REJECT_CONTRACT", ContractRejected));
//        }
//        NPC.MSGConversation.ShowResponses(list);
//    }

//    [ServerRpc(RequireOwnership = false, RunLocally = true)]
//    private void SendSetUpResponseCallbacks()
//    {
//        RpcWriter___Server_SendSetUpResponseCallbacks_2166136261();
//        RpcLogic___SendSetUpResponseCallbacks_2166136261();
//    }

//    [ObserversRpc(RunLocally = true)]
//    private void SetUpResponseCallbacks()
//    {
//        RpcWriter___Observers_SetUpResponseCallbacks_2166136261();
//        RpcLogic___SetUpResponseCallbacks_2166136261();
//    }

//    protected virtual void AcceptContractClicked()
//    {
//        if (OfferedContractInfo == null)
//        {
//            ScheduleOne.Console.LogWarning("Offered contract is null!");
//        }
//        else
//        {
//            PlayerSingleton<MessagesApp>.Instance.DealWindowSelector.SetIsOpen(open: true, NPC.MSGConversation, PlayerAcceptedContract);
//        }
//    }

//    protected virtual void CounterOfferClicked()
//    {
//        if (OfferedContractInfo == null)
//        {
//            NPC.MSGConversation.ClearResponses(network: true);
//            ScheduleOne.Console.LogWarning("Offered contract is null!");
//            return;
//        }
//        ProductDefinition item = Registry.GetItem<ProductDefinition>(OfferedContractInfo.Products.entries[0].ProductID);
//        int quantity = OfferedContractInfo.Products.entries[0].Quantity;
//        float payment = OfferedContractInfo.Payment;
//        PlayerSingleton<MessagesApp>.Instance.CounterofferInterface.Open(item, quantity, payment, NPC.MSGConversation, SendCounteroffer);
//    }

//    protected virtual void SendCounteroffer(ProductDefinition product, int quantity, float price)
//    {
//        if (OfferedContractInfo == null)
//        {
//            ScheduleOne.Console.LogWarning("Offered contract is null!");
//            return;
//        }
//        if (OfferedContractInfo.IsCounterOffer)
//        {
//            ScheduleOne.Console.LogWarning("Counter offer already sent");
//            return;
//        }
//        string text = "How about " + quantity + "x " + product.Name + " for " + MoneyManager.FormatAmount(price) + "?";
//        NPC.MSGConversation.SendMessage(new Message(text, Message.ESenderType.Player));
//        NPC.MSGConversation.ClearResponses();
//        ProcessCounterOfferServerSide(product.ID, quantity, price);
//    }

//    [ServerRpc(RequireOwnership = false)]
//    private void ProcessCounterOfferServerSide(string productID, int quantity, float price)
//    {
//        RpcWriter___Server_ProcessCounterOfferServerSide_900355577(productID, quantity, price);
//    }

//    [ObserversRpc(RunLocally = true)]
//    private void SetContractIsCounterOffer()
//    {
//        RpcWriter___Observers_SetContractIsCounterOffer_2166136261();
//        RpcLogic___SetContractIsCounterOffer_2166136261();
//    }

//    protected virtual void PlayerAcceptedContract(EDealWindow window)
//    {
//        ScheduleOne.Console.Log("Player accepted contract in window " + window);
//        if (OfferedContractInfo == null)
//        {
//            ScheduleOne.Console.LogWarning("Offered contract is null!");
//            return;
//        }
//        if (CurrentContract != null)
//        {
//            ScheduleOne.Console.LogWarning("Customer already has a contract!");
//            return;
//        }
//        if (NPC.MSGConversation != null)
//        {
//            string text = NPC.MSGConversation.GetResponse("ACCEPT_CONTRACT").text;
//            if (OfferedContractInfo.IsCounterOffer)
//            {
//                switch (window)
//                {
//                    case EDealWindow.Morning:
//                        text = "Morning";
//                        break;
//                    case EDealWindow.Afternoon:
//                        text = "Afternoon";
//                        break;
//                    case EDealWindow.Night:
//                        text = "Night";
//                        break;
//                    case EDealWindow.LateNight:
//                        text = "Late Night";
//                        break;
//                }
//            }
//            NPC.MSGConversation.SendMessage(new Message(text, Message.ESenderType.Player, _endOfGroup: true));
//            NPC.MSGConversation.ClearResponses(network: true);
//        }
//        else
//        {
//            ScheduleOne.Console.LogWarning("NPC.MSGConversation is null!");
//        }
//        DealWindowInfo windowInfo = DealWindowInfo.GetWindowInfo(window);
//        OfferedContractInfo.DeliveryWindow.WindowStartTime = windowInfo.StartTime;
//        OfferedContractInfo.DeliveryWindow.WindowEndTime = windowInfo.EndTime;
//        PlayContractAcceptedReaction();
//        SendContractAccepted(window, trackContract: true);
//        if (!InstanceFinder.IsServer)
//        {
//            OfferedContractInfo = null;
//        }
//    }

//    [ServerRpc(RequireOwnership = false)]
//    private void SendContractAccepted(EDealWindow window, bool trackContract)
//    {
//        RpcWriter___Server_SendContractAccepted_507093020(window, trackContract);
//    }

//    public virtual string ContractAccepted(EDealWindow window, bool trackContract)
//    {
//        if (OfferedContractInfo == null)
//        {
//            ScheduleOne.Console.LogWarning("Offered contract is null!");
//            return null;
//        }
//        DealWindowInfo windowInfo = DealWindowInfo.GetWindowInfo(window);
//        OfferedContractInfo.DeliveryWindow.WindowStartTime = windowInfo.StartTime;
//        OfferedContractInfo.DeliveryWindow.WindowEndTime = windowInfo.EndTime;
//        string text = GUIDManager.GenerateUniqueGUID().ToString();
//        NetworkSingleton<QuestManager>.Instance.SendContractAccepted(base.NetworkObject, OfferedContractInfo, trackContract, text);
//        ReceiveContractAccepted();
//        return text;
//    }

//    [ObserversRpc(RunLocally = true)]
//    private void ReceiveContractAccepted()
//    {
//        RpcWriter___Observers_ReceiveContractAccepted_2166136261();
//        RpcLogic___ReceiveContractAccepted_2166136261();
//    }

//    protected virtual void PlayContractAcceptedReaction()
//    {
//        DialogueChain chain = dialogueDatabase.GetChain(EDialogueModule.Customer, "contract_accepted");
//        chain = OfferedContractInfo.ProcessMessage(chain);
//        NPC.MSGConversation.SendMessageChain(chain.GetMessageChain(), 0.5f, notify: false);
//    }

//    protected virtual bool EvaluateCounteroffer(ProductDefinition product, int quantity, float price)
//    {
//        float adjustedWeeklySpend = customerData.GetAdjustedWeeklySpend(NPC.RelationData.RelationDelta / 5f);
//        List<EDay> orderDays = customerData.GetOrderDays(SyncAccessor_ < CurrentAddiction > k__BackingField, NPC.RelationData.RelationDelta / 5f);
//        float num = adjustedWeeklySpend / (float)orderDays.Count;
//        if (price >= num * 3f)
//        {
//            return false;
//        }
//        float valueProposition = GetValueProposition(Registry.GetItem<ProductDefinition>(OfferedContractInfo.Products.entries[0].ProductID), OfferedContractInfo.Payment / (float)OfferedContractInfo.Products.entries[0].Quantity);
//        float productEnjoyment = GetProductEnjoyment(product, customerData.Standards.GetCorrespondingQuality());
//        float num2 = Mathf.InverseLerp(-1f, 1f, productEnjoyment);
//        float valueProposition2 = GetValueProposition(product, price / (float)quantity);
//        float num3 = Mathf.Pow((float)quantity / (float)OfferedContractInfo.Products.entries[0].Quantity, 0.6f);
//        float num4 = Mathf.Lerp(0f, 2f, num3 * 0.5f);
//        float num5 = Mathf.Lerp(1f, 0f, Mathf.Abs(num4 - 1f));
//        if (valueProposition2 * num5 > valueProposition)
//        {
//            return true;
//        }
//        if (valueProposition2 < 0.12f)
//        {
//            return false;
//        }
//        float num6 = productEnjoyment * valueProposition;
//        float num7 = num2 * num5 * valueProposition2;
//        if (num7 > num6)
//        {
//            return true;
//        }
//        float num8 = num6 - num7;
//        float num9 = Mathf.Lerp(0f, 1f, num8 / 0.2f);
//        float t = Mathf.Max(SyncAccessor_ < CurrentAddiction > k__BackingField, NPC.RelationData.NormalizedRelationDelta);
//        float num10 = Mathf.Lerp(0f, 0.2f, t);
//        return UnityEngine.Random.Range(0f, 0.9f) + num10 > num9;
//    }

//    public static float GetValueProposition(ProductDefinition product, float price)
//    {
//        float num = product.MarketValue / price;
//        if (num < 1f)
//        {
//            num = Mathf.Pow(num, 2.5f);
//        }
//        return Mathf.Clamp(num, 0f, 2f);
//    }

//    protected virtual void ContractRejected()
//    {
//        if (OfferedContractInfo == null)
//        {
//            ScheduleOne.Console.LogWarning("Offered contract is null!");
//            return;
//        }
//        if (InstanceFinder.IsServer)
//        {
//            PlayContractRejectedReaction();
//            ReceiveContractRejected();
//        }
//        OfferedContractInfo = null;
//    }

//    [ObserversRpc(RunLocally = true)]
//    private void ReceiveContractRejected()
//    {
//        RpcWriter___Observers_ReceiveContractRejected_2166136261();
//        RpcLogic___ReceiveContractRejected_2166136261();
//    }

//    protected virtual void PlayContractRejectedReaction()
//    {
//        DialogueChain chain = dialogueDatabase.GetChain(EDialogueModule.Customer, "contract_rejected");
//        chain = OfferedContractInfo.ProcessMessage(chain);
//        NPC.MSGConversation.SendMessageChain(chain.GetMessageChain(), 0.5f, notify: false);
//    }

//    public virtual void SetIsAwaitingDelivery(bool awaiting)
//    {
//        IsAwaitingDelivery = awaiting;
//        if (awaiting && CurrentContract != null)
//        {
//            DealSignal.Location = CurrentContract.DeliveryLocation;
//            int min = ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(CurrentContract.DeliveryWindow.WindowEndTime, -60);
//            int num = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetTotalMinSum() - CurrentContract.AcceptTime.GetMinSum();
//            if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(min, CurrentContract.DeliveryWindow.WindowStartTime) && num > 300)
//            {
//                awaitingDealGreeting.Greeting = dialogueDatabase.GetLine(EDialogueModule.Customer, "late_deal");
//            }
//            else
//            {
//                awaitingDealGreeting.Greeting = dialogueDatabase.GetLine(EDialogueModule.Customer, "awaiting_deal");
//            }
//        }
//        if (awaitingDealGreeting != null)
//        {
//            awaitingDealGreeting.ShouldShow = awaiting;
//        }
//    }

//    public bool IsAtDealLocation()
//    {
//        if (CurrentContract == null)
//        {
//            return false;
//        }
//        if (!IsAwaitingDelivery)
//        {
//            return false;
//        }
//        if (!DealSignal.IsActive)
//        {
//            return false;
//        }
//        if (NPC.Movement.IsMoving)
//        {
//            return false;
//        }
//        return Vector3.Distance(base.transform.position, CurrentContract.DeliveryLocation.CustomerStandPoint.position) < 1f;
//    }

//    private void UpdatePotentialCustomerPoI()
//    {
//        if (!(potentialCustomerPoI == null))
//        {
//            potentialCustomerPoI.enabled = !NPC.RelationData.Unlocked && IsUnlockable();
//        }
//    }

//    public void SetPotentialCustomerPoIEnabled(bool enabled)
//    {
//        if (!(potentialCustomerPoI == null))
//        {
//            potentialCustomerPoI.enabled = enabled;
//        }
//    }

//    protected virtual bool ShouldTryGenerateDeal()
//    {
//        if (!NPC.RelationData.Unlocked)
//        {
//            return false;
//        }
//        if (CurrentContract != null)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " already has a contract");
//            }
//            return false;
//        }
//        if (OfferedContractInfo != null)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " already offered contract");
//            }
//            return false;
//        }
//        int num = 600 + NPC.FirstName[0] % 10 * 20;
//        if (TimeSinceLastDealCompleted < num)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " has not waited long enough since last deal");
//            }
//            return false;
//        }
//        if (TimeSinceLastDealOffered < num)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " has not waited long enough since last offer");
//            }
//            return false;
//        }
//        if (minsSinceUnlocked < 30)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " has not waited long enough since unlocked");
//            }
//            return false;
//        }
//        if (!NPC.IsConscious)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " is not conscious");
//            }
//            return false;
//        }
//        if (NPC.behaviour.RequestProductBehaviour.Active)
//        {
//            if (DEBUG)
//            {
//                ScheduleOne.Console.LogWarning("Customer " + NPC.fullName + " is already requesting a product");
//            }
//            return false;
//        }
//        return true;
//    }

//    public virtual void OfferDealItems(List<ItemInstance> items, bool offeredByPlayer, out bool accepted)
//    {
//        accepted = false;
//        if (!(CurrentContract == null))
//        {
//            int matchedProductCount;
//            float productListMatch = CurrentContract.GetProductListMatch(items, out matchedProductCount);
//            accepted = UnityEngine.Random.Range(0f, 1f) < productListMatch || GameManager.IS_TUTORIAL;
//            if (accepted || !offeredByPlayer)
//            {
//                ProcessHandover(HandoverScreen.EHandoverOutcome.Finalize, CurrentContract, items, offeredByPlayer);
//            }
//            else
//            {
//                CustomerRejectedDeal(offeredByPlayer);
//            }
//        }
//    }

//    public virtual void CustomerRejectedDeal(bool offeredByPlayer)
//    {
//        ScheduleOne.Console.Log("Customer rejected deal");
//        if (offeredByPlayer)
//        {
//            Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: true);
//        }
//        CurrentContract.Fail();
//        NPC.RelationData.ChangeRelationship(-0.5f);
//        NPC.PlayVO(EVOLineType.Annoyed);
//        NPC.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "deal_rejected", 30f);
//        NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "customer_rejected_deal"), 5f);
//        TimeSinceLastDealCompleted = 0;
//        if (NPC.RelationData.RelationDelta < 2.5f && offeredByPlayer && NPC.responses is NPCResponses_Civilian && NPC.Aggression > 0.5f && UnityEngine.Random.Range(0f, NPC.RelationData.NormalizedRelationDelta) < NPC.Aggression * 0.5f)
//        {
//            NPC.behaviour.CombatBehaviour.SetTarget(null, Player.GetClosestPlayer(base.transform.position, out var _).NetworkObject);
//            NPC.behaviour.CombatBehaviour.Enable_Networked(null);
//        }
//        Invoke("EndWait", 1f);
//    }

//    public virtual void ProcessHandover(HandoverScreen.EHandoverOutcome outcome, Contract contract, List<ItemInstance> items, bool handoverByPlayer, bool giveBonuses = true)
//    {
//        float highestAddiction;
//        EDrugType mainTypeType;
//        int matchedProductCount;
//        float satisfaction = Mathf.Clamp01(EvaluateDelivery(contract, items, out highestAddiction, out mainTypeType, out matchedProductCount));
//        ChangeAddiction(highestAddiction / 5f);
//        float relationDelta = NPC.RelationData.RelationDelta;
//        float relationshipChange = CustomerSatisfaction.GetRelationshipChange(satisfaction);
//        float change = relationshipChange * 0.2f * Mathf.Lerp(0.75f, 1.5f, highestAddiction);
//        AdjustAffinity(mainTypeType, change);
//        NPC.RelationData.ChangeRelationship(relationshipChange);
//        List<Contract.BonusPayment> list = new List<Contract.BonusPayment>();
//        if (giveBonuses)
//        {
//            if (NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive)
//            {
//                list.Add(new Contract.BonusPayment("Curfew Bonus", contract.Payment * 0.2f));
//            }
//            if (matchedProductCount > contract.ProductList.GetTotalQuantity())
//            {
//                list.Add(new Contract.BonusPayment("Generosity Bonus", 10f * (float)(matchedProductCount - contract.ProductList.GetTotalQuantity())));
//            }
//            GameDateTime acceptTime = contract.AcceptTime;
//            GameDateTime end = new GameDateTime(acceptTime.elapsedDays, ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(contract.DeliveryWindow.WindowStartTime, 60));
//            if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentDateWithinRange(acceptTime, end))
//            {
//                list.Add(new Contract.BonusPayment("Quick Delivery Bonus", contract.Payment * 0.1f));
//            }
//        }
//        float num = 0f;
//        foreach (Contract.BonusPayment item in list)
//        {
//            ScheduleOne.Console.Log("Bonus: " + item.Title + " Amount: " + item.Amount);
//            num += item.Amount;
//        }
//        if (handoverByPlayer)
//        {
//            Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: false);
//            contract.SubmitPayment(num);
//        }
//        if (outcome == HandoverScreen.EHandoverOutcome.Finalize && handoverByPlayer)
//        {
//            Singleton<DealCompletionPopup>.Instance.PlayPopup(this, satisfaction, relationDelta, contract.Payment, list);
//        }
//        TimeSinceLastDealCompleted = 0;
//        NPC.SendAnimationTrigger("GrabItem");
//        NetworkObject networkObject = null;
//        if (contract.Dealer != null)
//        {
//            networkObject = contract.Dealer.NetworkObject;
//        }
//        ScheduleOne.Console.Log("Base payment: " + contract.Payment + " Total bonus: " + num + " Satisfaction: " + satisfaction + " Dealer: " + networkObject?.name);
//        float totalPayment = Mathf.Clamp(contract.Payment + num, 0f, float.MaxValue);
//        ProcessHandoverServerSide(outcome, items, handoverByPlayer, totalPayment, contract.ProductList, satisfaction, networkObject);
//    }

//    [ServerRpc(RequireOwnership = false)]
//    private void ProcessHandoverServerSide(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, bool handoverByPlayer, float totalPayment, ProductList productList, float satisfaction, NetworkObject dealer)
//    {
//        RpcWriter___Server_ProcessHandoverServerSide_3760244802(outcome, items, handoverByPlayer, totalPayment, productList, satisfaction, dealer);
//    }

//    [ObserversRpc]
//    private void ProcessHandoverClient(float satisfaction, bool handoverByPlayer, string npcToRecommend)
//    {
//        RpcWriter___Observers_ProcessHandoverClient_537707335(satisfaction, handoverByPlayer, npcToRecommend);
//    }

//    public void ContractWellReceived(string npcToRecommend)
//    {
//        NPC nPC = null;
//        if (!string.IsNullOrEmpty(npcToRecommend))
//        {
//            nPC = NPCManager.GetNPC(npcToRecommend);
//        }
//        if (nPC != null)
//        {
//            if (nPC is Dealer)
//            {
//                RecommendDealer(nPC as Dealer);
//            }
//            else if (nPC is Supplier)
//            {
//                RecommendSupplier(nPC as Supplier);
//            }
//            else
//            {
//                RecommendCustomer(nPC.GetComponent<Customer>());
//            }
//        }
//        else
//        {
//            NPC.PlayVO(EVOLineType.Thanks);
//            NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "deal_completed"), 5f);
//            NPC.Avatar.EmotionManager.AddEmotionOverride("Cheery", "contract_done", 10f);
//        }
//    }

//    private void RecommendDealer(Dealer dealer)
//    {
//        if (dealer == null)
//        {
//            ScheduleOne.Console.LogWarning("Dealer is null!");
//            return;
//        }
//        ScheduleOne.Console.Log("Customer " + NPC.fullName + " recommended dealer " + dealer.fullName + " to player");
//        bool alreadyRecommended = dealer.HasBeenRecommended;
//        dealer.MarkAsRecommended();
//        DialogueContainer container;
//        if (Player.GetClosestPlayer(base.transform.position, out var _) == Player.Local)
//        {
//            string dialogueText = dialogueDatabase.GetLine(EDialogueModule.Customer, "post_deal_recommend_dealer").Replace("<NAME>", dealer.fullName);
//            container = new DialogueContainer();
//            DialogueNodeData dialogueNodeData = new DialogueNodeData();
//            dialogueNodeData.DialogueText = dialogueText;
//            dialogueNodeData.choices = new DialogueChoiceData[0];
//            dialogueNodeData.DialogueNodeLabel = "ENTRY";
//            dialogueNodeData.VoiceLine = EVOLineType.Thanks;
//            container.DialogueNodeData.Add(dialogueNodeData);
//            Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
//        }
//        IEnumerator Wait()
//        {
//            yield return new WaitForSeconds(0.1f);
//            NPC.dialogueHandler.InitializeDialogue(container);
//            yield return new WaitUntil(() => !NPC.dialogueHandler.IsPlaying);
//            if (!alreadyRecommended)
//            {
//                Singleton<HintDisplay>.Instance.ShowHint_20s("You can now hire <h1>" + dealer.fullName + "</h> as a dealer.");
//            }
//        }
//    }

//    private void RecommendSupplier(Supplier supplier)
//    {
//        if (supplier == null)
//        {
//            ScheduleOne.Console.LogWarning("Supplier is null!");
//            return;
//        }
//        ScheduleOne.Console.Log("Customer " + NPC.fullName + " recommended supplier " + supplier.fullName + " to player");
//        bool alreadyRecommended = supplier.RelationData.Unlocked;
//        supplier.SendUnlocked();
//        DialogueContainer container;
//        if (Player.GetClosestPlayer(base.transform.position, out var _) == Player.Local)
//        {
//            string supplierRecommendMessage = supplier.SupplierRecommendMessage;
//            container = new DialogueContainer();
//            DialogueNodeData dialogueNodeData = new DialogueNodeData();
//            dialogueNodeData.DialogueText = supplierRecommendMessage;
//            dialogueNodeData.choices = new DialogueChoiceData[0];
//            dialogueNodeData.DialogueNodeLabel = "ENTRY";
//            dialogueNodeData.VoiceLine = EVOLineType.Thanks;
//            container.DialogueNodeData.Add(dialogueNodeData);
//            Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
//        }
//        IEnumerator Wait()
//        {
//            yield return new WaitForSeconds(0.1f);
//            NPC.dialogueHandler.InitializeDialogue(container);
//            if (!alreadyRecommended)
//            {
//                Singleton<HintDisplay>.Instance.ShowHint_20s(supplier.SupplierUnlockHint);
//            }
//        }
//    }

//    private void RecommendCustomer(Customer friend)
//    {
//        if (friend == null)
//        {
//            ScheduleOne.Console.LogWarning("Friend is null!");
//            return;
//        }
//        ScheduleOne.Console.Log("Customer " + NPC.fullName + " recommended friend " + friend.NPC.fullName + " to player");
//        friend.SetHasBeenRecommended();
//        DialogueContainer container;
//        if (Player.GetClosestPlayer(base.transform.position, out var _) == Player.Local)
//        {
//            string text = dialogueDatabase.GetLine(EDialogueModule.Customer, "post_deal_recommend").Replace("<NAME>", friend.NPC.fullName);
//            text = text.Replace("they", friend.NPC.Avatar.GetThirdPersonAddress(capitalized: false));
//            text = text.Replace("them", friend.NPC.Avatar.GetThirdPersonPronoun(capitalized: false));
//            container = new DialogueContainer();
//            DialogueNodeData dialogueNodeData = new DialogueNodeData();
//            dialogueNodeData.DialogueText = text;
//            dialogueNodeData.choices = new DialogueChoiceData[0];
//            dialogueNodeData.DialogueNodeLabel = "ENTRY";
//            dialogueNodeData.VoiceLine = EVOLineType.Thanks;
//            container.DialogueNodeData.Add(dialogueNodeData);
//            Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
//        }
//        IEnumerator Wait()
//        {
//            yield return new WaitForSeconds(0.1f);
//            NPC.dialogueHandler.InitializeDialogue(container);
//        }
//    }

//    public virtual void CurrentContractEnded(EQuestState outcome)
//    {
//        if (outcome == EQuestState.Expired)
//        {
//            NPC.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "contract_expired", 30f);
//        }
//        ConfigureDealSignal(null, 0, active: false);
//        CurrentContract = null;
//    }

//    public virtual float EvaluateDelivery(Contract contract, List<ItemInstance> providedItems, out float highestAddiction, out EDrugType mainTypeType, out int matchedProductCount)
//    {
//        highestAddiction = 0f;
//        mainTypeType = EDrugType.Marijuana;
//        foreach (ProductList.Entry entry in contract.ProductList.entries)
//        {
//            List<ItemInstance> list = providedItems.Where((ItemInstance x) => x.ID == entry.ProductID).ToList();
//            List<ProductItemInstance> list2 = new List<ProductItemInstance>();
//            for (int i = 0; i < list.Count; i++)
//            {
//                list2.Add(list[i] as ProductItemInstance);
//            }
//            list2 = list2.OrderByDescending((ProductItemInstance x) => x.Quality).ToList();
//            int num = entry.Quantity;
//            for (int j = 0; j < list2.Count; j++)
//            {
//                if (num <= 0)
//                {
//                    break;
//                }
//                mainTypeType = (list2[j].Definition as ProductDefinition).DrugTypes[0].DrugType;
//                float addictiveness = (list2[j].Definition as ProductDefinition).GetAddictiveness();
//                if (addictiveness > highestAddiction)
//                {
//                    highestAddiction = addictiveness;
//                }
//                num--;
//            }
//        }
//        return contract.GetProductListMatch(providedItems, out matchedProductCount);
//    }

//    [ServerRpc(RequireOwnership = false)]
//    public void ChangeAddiction(float change)
//    {
//        RpcWriter___Server_ChangeAddiction_431000436(change);
//    }

//    private void ConsumeProduct(ItemInstance item)
//    {
//        Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
//        IEnumerator Wait()
//        {
//            yield return new WaitForSeconds(1.5f);
//            if (!(item is ProductItemInstance product))
//            {
//                ScheduleOne.Console.LogWarning("Item is not a product item instance");
//            }
//            else
//            {
//                NPC.behaviour.ConsumeProduct(product);
//            }
//        }
//    }

//    protected virtual bool ShowOfferDealOption(bool enabled)
//    {
//        if (Player.Local.CrimeData.CurrentPursuitLevel != 0 && Player.Local.CrimeData.TimeSinceSighted < 5f)
//        {
//            return false;
//        }
//        if (CurrentContract != null)
//        {
//            return false;
//        }
//        if (enabled && !IsAwaitingDelivery && NPC.RelationData.Unlocked)
//        {
//            return !NPC.behaviour.RequestProductBehaviour.Active;
//        }
//        return false;
//    }

//    protected virtual bool OfferDealValid(out string invalidReason)
//    {
//        invalidReason = string.Empty;
//        if (TimeSinceLastDealCompleted < 360)
//        {
//            invalidReason = "Customer recently completed a deal";
//            return false;
//        }
//        if (OfferedContractInfo != null)
//        {
//            invalidReason = "Customer already has a pending offer";
//            return false;
//        }
//        if (TimeSinceInstantDealOffered < 360 && !pendingInstantDeal)
//        {
//            invalidReason = "Already recently offered";
//            return false;
//        }
//        return true;
//    }

//    protected virtual void InstantDealOffered()
//    {
//        float num = Mathf.Clamp01((float)TimeSinceLastDealCompleted / 1440f) * 0.5f;
//        float num2 = NPC.RelationData.NormalizedRelationDelta * 0.3f;
//        float num3 = SyncAccessor_ < CurrentAddiction > k__BackingField * 0.2f;
//        float num4 = num + num2 + num3;
//        TimeSinceInstantDealOffered = 0;
//        if (UnityEngine.Random.Range(0f, 1f) < num4 || pendingInstantDeal)
//        {
//            NPC.PlayVO(EVOLineType.Acknowledge);
//            pendingInstantDeal = true;
//            NPC.dialogueHandler.SkipNextDialogueBehaviourEnd();
//            Singleton<HandoverScreen>.Instance.Open(null, this, HandoverScreen.EMode.Offer, HandoverClosed, GetOfferSuccessChance);
//        }
//        else
//        {
//            NPC.PlayVO(EVOLineType.No);
//            NPC.dialogueHandler.ShowWorldspaceDialogue_5s(dialogueDatabase.GetLine(EDialogueModule.Customer, "offer_reject"));
//        }
//        void HandoverClosed(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, float askingPrice)
//        {
//            TimeSinceInstantDealOffered = 0;
//            if (outcome == HandoverScreen.EHandoverOutcome.Cancelled)
//            {
//                EndWait();
//            }
//            else
//            {
//                pendingInstantDeal = false;
//                float offerSuccessChance = GetOfferSuccessChance(items, askingPrice);
//                if (UnityEngine.Random.value <= offerSuccessChance)
//                {
//                    Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: false);
//                    Contract contract = new Contract();
//                    ProductList productList = new ProductList();
//                    for (int i = 0; i < items.Count; i++)
//                    {
//                        if (items[i] is ProductItemInstance)
//                        {
//                            productList.entries.Add(new ProductList.Entry
//                            {
//                                ProductID = items[i].ID,
//                                Quantity = items[i].Quantity,
//                                Quality = CustomerData.Standards.GetCorrespondingQuality()
//                            });
//                        }
//                    }
//                    contract.SilentlyInitializeContract("Offer", string.Empty, null, string.Empty, base.NetworkObject, askingPrice, productList, string.Empty, new QuestWindowConfig(), 0, NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime());
//                    ProcessHandover(HandoverScreen.EHandoverOutcome.Finalize, contract, items, handoverByPlayer: true, giveBonuses: false);
//                }
//                else
//                {
//                    Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: true);
//                    NPC.dialogueHandler.ShowWorldspaceDialogue_5s(dialogueDatabase.GetLine(EDialogueModule.Customer, "sample_insufficient"));
//                    NPC.PlayVO(EVOLineType.Annoyed);
//                }
//                Invoke("EndWait", 1.5f);
//            }
//        }
//    }

//    public float GetOfferSuccessChance(List<ItemInstance> items, float askingPrice)
//    {
//        float adjustedWeeklySpend = CustomerData.GetAdjustedWeeklySpend(NPC.RelationData.RelationDelta / 5f);
//        List<EDay> orderDays = CustomerData.GetOrderDays(SyncAccessor_ < CurrentAddiction > k__BackingField, NPC.RelationData.RelationDelta / 5f);
//        float num = adjustedWeeklySpend / (float)orderDays.Count;
//        float num2 = 0f;
//        int num3 = 0;
//        for (int i = 0; i < items.Count; i++)
//        {
//            if (items[i] is ProductItemInstance)
//            {
//                ProductItemInstance productItemInstance = items[i] as ProductItemInstance;
//                if (!(productItemInstance.AppliedPackaging == null))
//                {
//                    float productEnjoyment = GetProductEnjoyment(items[i].Definition as ProductDefinition, productItemInstance.Quality);
//                    float num4 = Mathf.InverseLerp(-1f, 1f, productEnjoyment);
//                    num2 += num4 * (float)productItemInstance.Quantity * (float)productItemInstance.Amount;
//                    num3 += productItemInstance.Quantity * productItemInstance.Amount;
//                }
//            }
//        }
//        if (num3 == 0)
//        {
//            return 0f;
//        }
//        float num5 = num2 / (float)num3;
//        float price = askingPrice / (float)num3;
//        float num6 = 0f;
//        for (int j = 0; j < items.Count; j++)
//        {
//            if (items[j] is ProductItemInstance)
//            {
//                ProductItemInstance productItemInstance2 = items[j] as ProductItemInstance;
//                if (!(productItemInstance2.AppliedPackaging == null))
//                {
//                    float valueProposition = GetValueProposition(productItemInstance2.Definition as ProductDefinition, price);
//                    num6 += valueProposition * (float)productItemInstance2.Amount * (float)productItemInstance2.Quantity;
//                }
//            }
//        }
//        float f = num6 / (float)num3;
//        float num7 = askingPrice / num;
//        float item = 1f;
//        if (num7 > 1f)
//        {
//            float num8 = Mathf.Sqrt(num7);
//            item = Mathf.Clamp(1f - num8 / 4f, 0.01f, 1f);
//        }
//        float item2 = num5 + SyncAccessor_ < CurrentAddiction > k__BackingField * 0.25f;
//        float item3 = Mathf.Pow(f, 1.5f);
//        List<float> list = new List<float> { item2, item3, item };
//        list.Sort();
//        if (list[0] < 0.01f)
//        {
//            return 0f;
//        }
//        if (num7 > 3f)
//        {
//            return 0f;
//        }
//        return list[0] * 0.7f + list[1] * 0.2f + list[2] * 0.1f;
//    }

//    protected virtual bool ShouldTryApproachPlayer()
//    {
//        if (!NPC.RelationData.Unlocked)
//        {
//            return false;
//        }
//        if (CurrentContract != null)
//        {
//            return false;
//        }
//        if (OfferedContractInfo != null)
//        {
//            return false;
//        }
//        if (TimeSinceLastDealCompleted < 1440)
//        {
//            return false;
//        }
//        if (minsSinceUnlocked < 30)
//        {
//            return false;
//        }
//        if (!NPC.IsConscious)
//        {
//            return false;
//        }
//        if (AssignedDealer != null)
//        {
//            return false;
//        }
//        if (NPC.behaviour.RequestProductBehaviour.Active)
//        {
//            return false;
//        }
//        if (NPC.dialogueHandler.IsPlaying)
//        {
//            return false;
//        }
//        if (SyncAccessor_ < CurrentAddiction > k__BackingField < 0.33f)
//        {
//            return false;
//        }
//        if ((float)TimeSincePlayerApproached < Mathf.Lerp(4320f, 2160f, SyncAccessor_ < CurrentAddiction > k__BackingField))
//        {
//            return false;
//        }
//        if (OrderableProducts.Count == 0)
//        {
//            return false;
//        }
//        if (Player.GetClosestPlayer(base.transform.position, out var distance) == null)
//        {
//            return false;
//        }
//        if (distance < 20f)
//        {
//            return false;
//        }
//        for (int i = 0; i < UnlockedCustomers.Count; i++)
//        {
//            if (UnlockedCustomers[i].NPC.behaviour.RequestProductBehaviour.Active)
//            {
//                return false;
//            }
//        }
//        return true;
//    }

//    [Button]
//    public void RequestProduct()
//    {
//        RequestProduct(Player.GetRandomPlayer());
//    }

//    public void RequestProduct(Player target)
//    {
//        ScheduleOne.Console.Log(NPC.fullName + " is requesting product from " + target.PlayerName);
//        TimeSincePlayerApproached = 0;
//        NPC.behaviour.RequestProductBehaviour.AssignTarget(target.NetworkObject);
//        NPC.behaviour.RequestProductBehaviour.Enable_Networked(null);
//    }

//    public void PlayerRejectedProductRequest()
//    {
//        NPC.PlayVO(EVOLineType.Annoyed);
//        NPC.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "product_rejected", 30f, 1);
//        NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "request_product_rejected"), 5f);
//        if (NPC.responses is NPCResponses_Civilian && NPC.Aggression > 0.1f)
//        {
//            float num = Mathf.Clamp(NPC.Aggression, 0f, 0.7f);
//            num -= NPC.RelationData.NormalizedRelationDelta * 0.3f;
//            num += SyncAccessor_ < CurrentAddiction > k__BackingField * 0.2f;
//            if (UnityEngine.Random.Range(0f, 1f) < num)
//            {
//                NPC.behaviour.CombatBehaviour.SetTarget(null, Player.GetClosestPlayer(base.transform.position, out var _).NetworkObject);
//                NPC.behaviour.CombatBehaviour.Enable_Networked(null);
//            }
//        }
//    }

//    [ServerRpc(RequireOwnership = false)]
//    public void RejectProductRequestOffer()
//    {
//        RpcWriter___Server_RejectProductRequestOffer_2166136261();
//    }

//    [ObserversRpc(RunLocally = true)]
//    private void RejectProductRequestOffer_Local()
//    {
//        RpcWriter___Observers_RejectProductRequestOffer_Local_2166136261();
//        RpcLogic___RejectProductRequestOffer_Local_2166136261();
//    }

//    public void AssignDealer(Dealer dealer)
//    {
//        AssignedDealer = dealer;
//    }

//    public virtual string GetSaveString()
//    {
//        return GetCustomerData().GetJson();
//    }

//    public ScheduleOne.Persistence.Datas.CustomerData GetCustomerData()
//    {
//        string[] array = new string[OrderableProducts.Count];
//        for (int i = 0; i < OrderableProducts.Count; i++)
//        {
//            array[i] = OrderableProducts[i].ID;
//        }
//        float[] array2 = new float[currentAffinityData.ProductAffinities.Count];
//        for (int j = 0; j < currentAffinityData.ProductAffinities.Count; j++)
//        {
//            array2[j] = currentAffinityData.ProductAffinities[j].Affinity;
//        }
//        return new ScheduleOne.Persistence.Datas.CustomerData(SyncAccessor_ < CurrentAddiction > k__BackingField, array, array2, TimeSinceLastDealCompleted, TimeSinceLastDealOffered, OfferedDeals, CompletedDeliveries, OfferedContractInfo != null, OfferedContractInfo, OfferedContractTime, TimeSincePlayerApproached, TimeSinceInstantDealOffered, SyncAccessor_ < HasBeenRecommended > k__BackingField);
//    }

//    public virtual List<string> WriteData(string parentFolderPath)
//    {
//        return new List<string>();
//    }

//    [TargetRpc]
//    private void ReceiveCustomerData(NetworkConnection conn, ScheduleOne.Persistence.Datas.CustomerData data)
//    {
//        RpcWriter___Target_ReceiveCustomerData_2280244125(conn, data);
//    }

//    public virtual void Load(ScheduleOne.Persistence.Datas.CustomerData data)
//    {
//        CurrentAddiction = data.Dependence;
//        for (int i = 0; i < currentAffinityData.ProductAffinities.Count; i++)
//        {
//            if (i >= currentAffinityData.ProductAffinities.Count)
//            {
//                ScheduleOne.Console.LogWarning("Product affinities array is too short");
//                break;
//            }
//            if (data.ProductAffinities.Length <= i || float.IsNaN(data.ProductAffinities[i]))
//            {
//                ScheduleOne.Console.LogWarning("Product affinity is NaN");
//            }
//            else
//            {
//                currentAffinityData.ProductAffinities[i].Affinity = data.ProductAffinities[i];
//            }
//        }
//        TimeSinceLastDealCompleted = data.TimeSinceLastDealCompleted;
//        TimeSinceLastDealOffered = data.TimeSinceLastDealOffered;
//        OfferedDeals = data.OfferedDeals;
//        CompletedDeliveries = data.CompletedDeals;
//        _ = data.TimeSincePlayerApproached;
//        TimeSincePlayerApproached = data.TimeSincePlayerApproached;
//        _ = data.TimeSinceInstantDealOffered;
//        TimeSinceInstantDealOffered = data.TimeSinceInstantDealOffered;
//        _ = data.HasBeenRecommended;
//        HasBeenRecommended = data.HasBeenRecommended;
//        if (data.IsContractOffered && data.OfferedContract != null)
//        {
//            OfferedContractInfo = data.OfferedContract;
//            OfferedContractTime = data.OfferedContractTime;
//            SetUpResponseCallbacks();
//        }
//    }

//    protected virtual bool IsReadyForHandover(bool enabled)
//    {
//        if (Player.Local.CrimeData.CurrentPursuitLevel != 0 && Player.Local.CrimeData.TimeSinceSighted < 5f)
//        {
//            return false;
//        }
//        if (enabled)
//        {
//            return IsAwaitingDelivery;
//        }
//        return false;
//    }

//    protected virtual bool IsHandoverChoiceValid(out string invalidReason)
//    {
//        invalidReason = string.Empty;
//        if (CurrentContract == null)
//        {
//            return false;
//        }
//        if (AssignedDealer != null && (AssignedDealer.ActiveContracts.Contains(CurrentContract) || CurrentContract.Dealer != null))
//        {
//            invalidReason = "Customer is waiting for a dealer";
//            return false;
//        }
//        return true;
//    }

//    public void HandoverChosen()
//    {
//        NPC.dialogueHandler.SkipNextDialogueBehaviourEnd();
//        Singleton<HandoverScreen>.Instance.Open(CurrentContract, this, HandoverScreen.EMode.Contract, delegate (HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, float price)
//        {
//            if (outcome == HandoverScreen.EHandoverOutcome.Finalize)
//            {
//                OfferDealItems(items, offeredByPlayer: true, out var _);
//            }
//            else
//            {
//                EndWait();
//            }
//        }, null);
//    }

//    protected virtual bool ShowDirectApproachOption(bool enabled)
//    {
//        if (Player.Local.CrimeData.CurrentPursuitLevel != 0 && Player.Local.CrimeData.TimeSinceSighted < 5f)
//        {
//            return false;
//        }
//        if (enabled && customerData.CanBeDirectlyApproached && !IsAwaitingDelivery)
//        {
//            return !NPC.RelationData.Unlocked;
//        }
//        return false;
//    }

//    public virtual bool IsUnlockable()
//    {
//        if (NPC.RelationData.Unlocked)
//        {
//            return false;
//        }
//        if (!GameManager.IS_TUTORIAL && !Singleton<Map>.Instance.GetRegionData(NPC.Region).IsUnlocked)
//        {
//            return false;
//        }
//        if (!NPC.RelationData.IsMutuallyKnown())
//        {
//            return false;
//        }
//        return true;
//    }

//    protected virtual bool SampleOptionValid(out string invalidReason)
//    {
//        if (!GameManager.IS_TUTORIAL)
//        {
//            MapRegionData regionData = Singleton<Map>.Instance.GetRegionData(NPC.Region);
//            if (!regionData.IsUnlocked)
//            {
//                invalidReason = "'" + regionData.Name + "' region must be unlocked";
//                return false;
//            }
//        }
//        if (!NPC.RelationData.IsMutuallyKnown())
//        {
//            invalidReason = "Unlock one of " + NPC.FirstName + "'s connections first";
//            return false;
//        }
//        if (GetSampleRequestSuccessChance() == 0f)
//        {
//            invalidReason = "Mutual relationship too low";
//            return false;
//        }
//        if (sampleOfferedToday)
//        {
//            invalidReason = "Sample already offered today";
//            return false;
//        }
//        invalidReason = string.Empty;
//        return true;
//    }

//    public bool KnownAndRecommended()
//    {
//        if (!GameManager.IS_TUTORIAL && !Singleton<Map>.Instance.GetRegionData(NPC.Region).IsUnlocked)
//        {
//            return false;
//        }
//        if (!SyncAccessor_ < HasBeenRecommended > k__BackingField)
//        {
//            return false;
//        }
//        if (!NPC.RelationData.IsMutuallyKnown())
//        {
//            return false;
//        }
//        return true;
//    }

//    public void SampleOffered()
//    {
//        if (awaitingSample)
//        {
//            SampleAccepted();
//            return;
//        }
//        float sampleRequestSuccessChance = GetSampleRequestSuccessChance();
//        if (UnityEngine.Random.Range(0f, 1f) <= sampleRequestSuccessChance)
//        {
//            SampleAccepted();
//            return;
//        }
//        DirectApproachRejected();
//        sampleOfferedToday = true;
//    }

//    protected virtual float GetSampleRequestSuccessChance()
//    {
//        if (NPC.RelationData.Unlocked)
//        {
//            return 1f;
//        }
//        if (NPC.RelationData.IsMutuallyKnown())
//        {
//            return 1f;
//        }
//        if (customerData.GuaranteeFirstSampleSuccess)
//        {
//            return 1f;
//        }
//        if (NetworkSingleton<GameManager>.Instance.IsTutorial)
//        {
//            return 1f;
//        }
//        return Mathf.InverseLerp(customerData.MinMutualRelationRequirement, customerData.MaxMutualRelationRequirement, NPC.RelationData.GetAverageMutualRelationship());
//    }

//    protected virtual void SampleAccepted()
//    {
//        awaitingSample = true;
//        NPC.dialogueHandler.SkipNextDialogueBehaviourEnd();
//        NPC.PlayVO(EVOLineType.Acknowledge);
//        Singleton<HandoverScreen>.Instance.Open(null, this, HandoverScreen.EMode.Sample, ProcessSample, GetSampleSuccess);
//    }

//    private float GetSampleSuccess(List<ItemInstance> items, float price)
//    {
//        float num = -1000f;
//        foreach (ItemInstance item in items)
//        {
//            if (item is ProductItemInstance)
//            {
//                ProductItemInstance productItemInstance = item as ProductItemInstance;
//                float productEnjoyment = GetProductEnjoyment(item.Definition as ProductDefinition, productItemInstance.Quality);
//                if (productEnjoyment > num)
//                {
//                    num = productEnjoyment;
//                }
//            }
//        }
//        float num2 = NPC.RelationData.RelationDelta / 5f;
//        if (num2 >= 0.5f)
//        {
//            num += Mathf.Lerp(0f, 0.2f, (num2 - 0.5f) * 2f);
//        }
//        num += Mathf.Lerp(0f, 0.2f, SyncAccessor_ < CurrentAddiction > k__BackingField);
//        float num3 = NPC.RelationData.GetAverageMutualRelationship() / 5f;
//        if (num3 > 0.5f)
//        {
//            num += Mathf.Lerp(0f, 0.2f, (num3 - 0.5f) * 2f);
//        }
//        num = Mathf.Clamp01(num);
//        if (num <= 0f)
//        {
//            return 0f;
//        }
//        return NetworkSingleton<ProductManager>.Instance.SampleSuccessCurve.Evaluate(num);
//    }

//    private void ProcessSample(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, float price)
//    {
//        if (outcome == HandoverScreen.EHandoverOutcome.Cancelled)
//        {
//            Invoke("EndWait", 1.5f);
//            return;
//        }
//        Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: false);
//        awaitingSample = false;
//        ProcessSampleServerSide(items);
//    }

//    [ServerRpc(RequireOwnership = false, RunLocally = true)]
//    private void ProcessSampleServerSide(List<ItemInstance> items)
//    {
//        RpcWriter___Server_ProcessSampleServerSide_3704012609(items);
//        RpcLogic___ProcessSampleServerSide_3704012609(items);
//    }

//    [ObserversRpc(RunLocally = true)]
//    private void ProcessSampleClient()
//    {
//        RpcWriter___Observers_ProcessSampleClient_2166136261();
//        RpcLogic___ProcessSampleClient_2166136261();
//    }

//    private void SampleConsumed()
//    {
//        NPC.behaviour.ConsumeProductBehaviour.onConsumeDone.RemoveListener(SampleConsumed);
//        NPC.behaviour.GenericDialogueBehaviour.SendEnable();
//        if (consumedSample == null)
//        {
//            ScheduleOne.Console.LogWarning("Consumed sample is null");
//            return;
//        }
//        float sampleSuccess = GetSampleSuccess(new List<ItemInstance> { consumedSample }, 0f);
//        if (UnityEngine.Random.Range(0f, 1f) <= sampleSuccess || NetworkSingleton<GameManager>.Instance.IsTutorial || customerData.GuaranteeFirstSampleSuccess)
//        {
//            NetworkSingleton<LevelManager>.Instance.AddXP(50);
//            NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("SuccessfulSampleCount", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("SuccessfulSampleCount") + 1f).ToString());
//            SampleWasSufficient();
//        }
//        else
//        {
//            SampleWasInsufficient();
//            float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("SampleRejectionCount");
//            NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("SampleRejectionCount", (value + 1f).ToString());
//        }
//        consumedSample = null;
//        Invoke("EndWait", 1.5f);
//    }

//    private void EndWait()
//    {
//        if (!NPC.dialogueHandler.IsPlaying && !(Singleton<HandoverScreen>.Instance.CurrentCustomer == this))
//        {
//            NPC.behaviour.GenericDialogueBehaviour.SendDisable();
//        }
//    }

//    protected virtual void DirectApproachRejected()
//    {
//        if (UnityEngine.Random.Range(0f, 1f) <= customerData.CallPoliceChance)
//        {
//            NPC.PlayVO(EVOLineType.Angry);
//            NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "sample_offer_rejected_police"), 5f);
//            NPC.actions.SetCallPoliceBehaviourCrime(new AttemptingToSell());
//            NPC.actions.CallPolice_Networked(Player.Local);
//        }
//        else
//        {
//            NPC.PlayVO(EVOLineType.Annoyed);
//            NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "sample_offer_rejected"), 5f);
//        }
//    }

//    [ObserversRpc]
//    private void SampleWasSufficient()
//    {
//        RpcWriter___Observers_SampleWasSufficient_2166136261();
//    }

//    [ObserversRpc]
//    private void SampleWasInsufficient()
//    {
//        RpcWriter___Observers_SampleWasInsufficient_2166136261();
//    }

//    public float GetProductEnjoyment(ProductDefinition product, EQuality quality)
//    {
//        float num = 0f;
//        for (int i = 0; i < product.DrugTypes.Count; i++)
//        {
//            num += currentAffinityData.GetAffinity(product.DrugTypes[i].DrugType) * 0.3f;
//        }
//        float num2 = 0f;
//        int j;
//        for (j = 0; j < customerData.PreferredProperties.Count; j++)
//        {
//            if (product.Properties.Find((Property x) => x == customerData.PreferredProperties[j]) != null)
//            {
//                num2 += 1f / (float)customerData.PreferredProperties.Count;
//            }
//        }
//        num += num2 * 0.4f;
//        float qualityScalar = ScheduleOne.Economy.CustomerData.GetQualityScalar(quality);
//        float qualityScalar2 = ScheduleOne.Economy.CustomerData.GetQualityScalar(customerData.Standards.GetCorrespondingQuality());
//        float num3 = qualityScalar - qualityScalar2;
//        float num4 = 0f;
//        num4 = ((num3 >= 0.25f) ? 1f : ((num3 >= 0f) ? 0.5f : ((!(num3 >= -0.25f)) ? (-1f) : (-0.5f))));
//        num += num4 * 0.3f;
//        float b = 1f;
//        return Mathf.InverseLerp(-0.6f, b, num);
//    }

//    public List<EDrugType> GetOrderedDrugTypes()
//    {
//        List<EDrugType> list = new List<EDrugType>();
//        for (int i = 0; i < currentAffinityData.ProductAffinities.Count; i++)
//        {
//            list.Add(currentAffinityData.ProductAffinities[i].DrugType);
//        }
//        return list.OrderByDescending((EDrugType x) => currentAffinityData.ProductAffinities.Find((ProductTypeAffinity y) => y.DrugType == x).Affinity).ToList();
//    }

//    [ServerRpc(RequireOwnership = false)]
//    public void AdjustAffinity(EDrugType drugType, float change)
//    {
//        RpcWriter___Server_AdjustAffinity_3036964899(drugType, change);
//    }

//    [Button]
//    public void AutocreateCustomerSettings()
//    {
//        if (customerData != null)
//        {
//            ScheduleOne.Console.LogWarning("Customer data already exists");
//        }
//    }

//    public virtual void NetworkInitialize___Early()
//    {
//        if (!NetworkInitialize___EarlyScheduleOne.Economy.CustomerAssembly - CSharp.dll_Excuted)
//        {
//            NetworkInitialize___EarlyScheduleOne.Economy.CustomerAssembly - CSharp.dll_Excuted = true;
//            syncVar___<HasBeenRecommended> k__BackingField = new SyncVar<bool>(this, 1u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, HasBeenRecommended);
//            syncVar___<CurrentAddiction> k__BackingField = new SyncVar<float>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentAddiction);
//            RegisterObserversRpc(0u, RpcReader___Observers_ConfigureDealSignal_338960014);
//            RegisterTargetRpc(1u, RpcReader___Target_ConfigureDealSignal_338960014);
//            RegisterObserversRpc(2u, RpcReader___Observers_SetOfferedContract_4277245194);
//            RegisterServerRpc(3u, RpcReader___Server_ExpireOffer_2166136261);
//            RegisterServerRpc(4u, RpcReader___Server_SendSetUpResponseCallbacks_2166136261);
//            RegisterObserversRpc(5u, RpcReader___Observers_SetUpResponseCallbacks_2166136261);
//            RegisterServerRpc(6u, RpcReader___Server_ProcessCounterOfferServerSide_900355577);
//            RegisterObserversRpc(7u, RpcReader___Observers_SetContractIsCounterOffer_2166136261);
//            RegisterServerRpc(8u, RpcReader___Server_SendContractAccepted_507093020);
//            RegisterObserversRpc(9u, RpcReader___Observers_ReceiveContractAccepted_2166136261);
//            RegisterObserversRpc(10u, RpcReader___Observers_ReceiveContractRejected_2166136261);
//            RegisterServerRpc(11u, RpcReader___Server_ProcessHandoverServerSide_3760244802);
//            RegisterObserversRpc(12u, RpcReader___Observers_ProcessHandoverClient_537707335);
//            RegisterServerRpc(13u, RpcReader___Server_ChangeAddiction_431000436);
//            RegisterServerRpc(14u, RpcReader___Server_RejectProductRequestOffer_2166136261);
//            RegisterObserversRpc(15u, RpcReader___Observers_RejectProductRequestOffer_Local_2166136261);
//            RegisterTargetRpc(16u, RpcReader___Target_ReceiveCustomerData_2280244125);
//            RegisterServerRpc(17u, RpcReader___Server_ProcessSampleServerSide_3704012609);
//            RegisterObserversRpc(18u, RpcReader___Observers_ProcessSampleClient_2166136261);
//            RegisterObserversRpc(19u, RpcReader___Observers_SampleWasSufficient_2166136261);
//            RegisterObserversRpc(20u, RpcReader___Observers_SampleWasInsufficient_2166136261);
//            RegisterServerRpc(21u, RpcReader___Server_AdjustAffinity_3036964899);
//            RegisterSyncVarRead(ReadSyncVar___ScheduleOne.Economy.Customer);
//        }
//    }

//    public virtual void NetworkInitialize__Late()
//    {
//        if (!NetworkInitialize__LateScheduleOne.Economy.CustomerAssembly - CSharp.dll_Excuted)
//        {
//            NetworkInitialize__LateScheduleOne.Economy.CustomerAssembly - CSharp.dll_Excuted = true;
//            syncVar___<HasBeenRecommended> k__BackingField.SetRegistered();
//            syncVar___<CurrentAddiction> k__BackingField.SetRegistered();
//        }
//    }

//    public override void NetworkInitializeIfDisabled()
//    {
//        NetworkInitialize___Early();
//        NetworkInitialize__Late();
//    }

//    private void RpcWriter___Observers_ConfigureDealSignal_338960014(NetworkConnection conn, int startTime, bool active)
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            writer.WriteInt32(startTime);
//            writer.WriteBoolean(active);
//            SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ConfigureDealSignal_338960014(NetworkConnection conn, int startTime, bool active)
//    {
//        DealSignal.SetStartTime(startTime);
//        DealSignal.gameObject.SetActive(active);
//    }

//    private void RpcReader___Observers_ConfigureDealSignal_338960014(PooledReader PooledReader0, Channel channel)
//    {
//        int startTime = PooledReader0.ReadInt32();
//        bool active = PooledReader0.ReadBoolean();
//        if (base.IsClientInitialized && !base.IsHost)
//        {
//            RpcLogic___ConfigureDealSignal_338960014(null, startTime, active);
//        }
//    }

//    private void RpcWriter___Target_ConfigureDealSignal_338960014(NetworkConnection conn, int startTime, bool active)
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            writer.WriteInt32(startTime);
//            writer.WriteBoolean(active);
//            SendTargetRpc(1u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
//            writer.Store();
//        }
//    }

//    private void RpcReader___Target_ConfigureDealSignal_338960014(PooledReader PooledReader0, Channel channel)
//    {
//        int startTime = PooledReader0.ReadInt32();
//        bool active = PooledReader0.ReadBoolean();
//        if (base.IsClientInitialized)
//        {
//            RpcLogic___ConfigureDealSignal_338960014(base.LocalConnection, startTime, active);
//        }
//    }

//    private void RpcWriter___Observers_SetOfferedContract_4277245194(ContractInfo info, GameDateTime offerTime)
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            GeneratedWriters___Internal.Write___ScheduleOne.Quests.ContractInfoFishNet.Serializing.Generated(writer, info);
//            GeneratedWriters___Internal.Write___ScheduleOne.GameTime.GameDateTimeFishNet.Serializing.Generated(writer, offerTime);
//            SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___SetOfferedContract_4277245194(ContractInfo info, GameDateTime offerTime)
//    {
//        OfferedContractInfo = info;
//        OfferedContractTime = offerTime;
//        TimeSinceLastDealOffered = 0;
//    }

//    private void RpcReader___Observers_SetOfferedContract_4277245194(PooledReader PooledReader0, Channel channel)
//    {
//        ContractInfo info = GeneratedReaders___Internal.Read___ScheduleOne.Quests.ContractInfoFishNet.Serializing.Generateds(PooledReader0);
//        GameDateTime offerTime = GeneratedReaders___Internal.Read___ScheduleOne.GameTime.GameDateTimeFishNet.Serializing.Generateds(PooledReader0);
//        if (base.IsClientInitialized)
//        {
//            RpcLogic___SetOfferedContract_4277245194(info, offerTime);
//        }
//    }

//    private void RpcWriter___Server_ExpireOffer_2166136261()
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendServerRpc(3u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    public virtual void RpcLogic___ExpireOffer_2166136261()
//    {
//        if (OfferedContractInfo != null)
//        {
//            NPC.MSGConversation.SendMessageChain(NPC.dialogueHandler.Database.GetChain(EDialogueModule.Customer, "offer_expired").GetMessageChain());
//            NPC.MSGConversation.ClearResponses(network: true);
//            OfferedContractInfo = null;
//        }
//    }

//    private void RpcReader___Server_ExpireOffer_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        if (base.IsServerInitialized && !conn.IsLocalClient)
//        {
//            RpcLogic___ExpireOffer_2166136261();
//        }
//    }

//    private void RpcWriter___Server_SendSetUpResponseCallbacks_2166136261()
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendServerRpc(4u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___SendSetUpResponseCallbacks_2166136261()
//    {
//        SetUpResponseCallbacks();
//    }

//    private void RpcReader___Server_SendSetUpResponseCallbacks_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        if (base.IsServerInitialized && !conn.IsLocalClient)
//        {
//            RpcLogic___SendSetUpResponseCallbacks_2166136261();
//        }
//    }

//    private void RpcWriter___Observers_SetUpResponseCallbacks_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___SetUpResponseCallbacks_2166136261()
//    {
//        if (NPC.MSGConversation == null)
//        {
//            return;
//        }
//        for (int i = 0; i < NPC.MSGConversation.currentResponses.Count; i++)
//        {
//            if (NPC.MSGConversation.currentResponses[i].label == "ACCEPT_CONTRACT")
//            {
//                NPC.MSGConversation.currentResponses[i].disableDefaultResponseBehaviour = true;
//                NPC.MSGConversation.currentResponses[i].callback = AcceptContractClicked;
//            }
//            else if (NPC.MSGConversation.currentResponses[i].label == "REJECT_CONTRACT")
//            {
//                NPC.MSGConversation.currentResponses[i].callback = ContractRejected;
//            }
//            else if (NPC.MSGConversation.currentResponses[i].label == "COUNTEROFFER")
//            {
//                NPC.MSGConversation.currentResponses[i].callback = CounterOfferClicked;
//                NPC.MSGConversation.currentResponses[i].disableDefaultResponseBehaviour = true;
//            }
//        }
//    }

//    private void RpcReader___Observers_SetUpResponseCallbacks_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized && !base.IsHost)
//        {
//            RpcLogic___SetUpResponseCallbacks_2166136261();
//        }
//    }

//    private void RpcWriter___Server_ProcessCounterOfferServerSide_900355577(string productID, int quantity, float price)
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            writer.WriteString(productID);
//            writer.WriteInt32(quantity);
//            writer.WriteSingle(price);
//            SendServerRpc(6u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ProcessCounterOfferServerSide_900355577(string productID, int quantity, float price)
//    {
//        ProductDefinition item = Registry.GetItem<ProductDefinition>(productID);
//        if (item == null)
//        {
//            ScheduleOne.Console.LogError("Product is null!");
//            return;
//        }
//        if (EvaluateCounteroffer(item, quantity, price))
//        {
//            NetworkSingleton<LevelManager>.Instance.AddXP(5);
//            DialogueChain chain = dialogueDatabase.GetChain(EDialogueModule.Customer, "counteroffer_accepted");
//            NPC.MSGConversation.SendMessageChain(chain.GetMessageChain(), 1f, notify: false);
//            OfferedContractInfo.Payment = price;
//            OfferedContractInfo.Products.entries[0].ProductID = item.ID;
//            OfferedContractInfo.Products.entries[0].Quantity = quantity;
//            SetContractIsCounterOffer();
//            List<Response> list = new List<Response>();
//            list.Add(new Response("[Schedule Deal]", "ACCEPT_CONTRACT", AcceptContractClicked, _disableDefaultResponseBehaviour: true));
//            list.Add(new Response("Nevermind", "REJECT_CONTRACT", ContractRejected));
//            NPC.MSGConversation.ShowResponses(list, 1f);
//        }
//        else
//        {
//            DialogueChain chain2 = dialogueDatabase.GetChain(EDialogueModule.Customer, "counteroffer_rejected");
//            NPC.MSGConversation.SendMessageChain(chain2.GetMessageChain(), 0.8f, notify: false);
//            OfferedContractInfo = null;
//            NPC.MSGConversation.ClearResponses(network: true);
//        }
//        HasChanged = true;
//    }

//    private void RpcReader___Server_ProcessCounterOfferServerSide_900355577(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        string productID = PooledReader0.ReadString();
//        int quantity = PooledReader0.ReadInt32();
//        float price = PooledReader0.ReadSingle();
//        if (base.IsServerInitialized)
//        {
//            RpcLogic___ProcessCounterOfferServerSide_900355577(productID, quantity, price);
//        }
//    }

//    private void RpcWriter___Observers_SetContractIsCounterOffer_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___SetContractIsCounterOffer_2166136261()
//    {
//        if (OfferedContractInfo != null)
//        {
//            OfferedContractInfo.IsCounterOffer = true;
//        }
//    }

//    private void RpcReader___Observers_SetContractIsCounterOffer_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized && !base.IsHost)
//        {
//            RpcLogic___SetContractIsCounterOffer_2166136261();
//        }
//    }

//    private void RpcWriter___Server_SendContractAccepted_507093020(EDealWindow window, bool trackContract)
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            GeneratedWriters___Internal.Write___ScheduleOne.Economy.EDealWindowFishNet.Serializing.Generated(writer, window);
//            writer.WriteBoolean(trackContract);
//            SendServerRpc(8u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___SendContractAccepted_507093020(EDealWindow window, bool trackContract)
//    {
//        ContractAccepted(window, trackContract);
//    }

//    private void RpcReader___Server_SendContractAccepted_507093020(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        EDealWindow window = GeneratedReaders___Internal.Read___ScheduleOne.Economy.EDealWindowFishNet.Serializing.Generateds(PooledReader0);
//        bool trackContract = PooledReader0.ReadBoolean();
//        if (base.IsServerInitialized)
//        {
//            RpcLogic___SendContractAccepted_507093020(window, trackContract);
//        }
//    }

//    private void RpcWriter___Observers_ReceiveContractAccepted_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ReceiveContractAccepted_2166136261()
//    {
//        OfferedContractInfo = null;
//    }

//    private void RpcReader___Observers_ReceiveContractAccepted_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized && !base.IsHost)
//        {
//            RpcLogic___ReceiveContractAccepted_2166136261();
//        }
//    }

//    private void RpcWriter___Observers_ReceiveContractRejected_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(10u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ReceiveContractRejected_2166136261()
//    {
//        OfferedContractInfo = null;
//    }

//    private void RpcReader___Observers_ReceiveContractRejected_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized && !base.IsHost)
//        {
//            RpcLogic___ReceiveContractRejected_2166136261();
//        }
//    }

//    private void RpcWriter___Server_ProcessHandoverServerSide_3760244802(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, bool handoverByPlayer, float totalPayment, ProductList productList, float satisfaction, NetworkObject dealer)
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            GeneratedWriters___Internal.Write___ScheduleOne.UI.Handover.HandoverScreen / EHandoverOutcomeFishNet.Serializing.Generated(writer, outcome);
//            GeneratedWriters___Internal.Write___System.Collections.Generic.List`1 < ScheduleOne.ItemFramework.ItemInstance > FishNet.Serializing.Generated(writer, items);
//            writer.WriteBoolean(handoverByPlayer);
//            writer.WriteSingle(totalPayment);
//            GeneratedWriters___Internal.Write___ScheduleOne.Product.ProductListFishNet.Serializing.Generated(writer, productList);
//            writer.WriteSingle(satisfaction);
//            writer.WriteNetworkObject(dealer);
//            SendServerRpc(11u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ProcessHandoverServerSide_3760244802(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, bool handoverByPlayer, float totalPayment, ProductList productList, float satisfaction, NetworkObject dealer)
//    {
//        CompletedDeliveries++;
//        Invoke("EndWait", 1.5f);
//        if (handoverByPlayer)
//        {
//            List<string> list = new List<string>();
//            List<int> list2 = new List<int>();
//            foreach (ProductList.Entry entry in productList.entries)
//            {
//                list.Add(entry.ProductID);
//                list2.Add(entry.Quantity);
//            }
//            for (int i = 0; i < list.Count; i++)
//            {
//                NetworkSingleton<DailySummary>.Instance.AddSoldItem(list[i], list2[i]);
//            }
//            NetworkSingleton<DailySummary>.Instance.AddPlayerMoney(totalPayment);
//            NetworkSingleton<LevelManager>.Instance.AddXP(20);
//        }
//        else
//        {
//            NetworkSingleton<LevelManager>.Instance.AddXP(10);
//            NetworkSingleton<DailySummary>.Instance.AddDealerMoney(totalPayment);
//            if (dealer != null)
//            {
//                dealer.GetComponent<Dealer>().CompletedDeal();
//                dealer.GetComponent<Dealer>().SubmitPayment(totalPayment);
//            }
//        }
//        NetworkSingleton<MoneyManager>.Instance.ChangeLifetimeEarnings(totalPayment);
//        if (CurrentContract != null)
//        {
//            CurrentContract.Complete();
//        }
//        foreach (ItemInstance item in items)
//        {
//            NPC.Inventory.InsertItem(item);
//        }
//        if (items.Count > 0)
//        {
//            ConsumeProduct(items[0]);
//        }
//        if (NPC.RelationData.NormalizedRelationDelta >= 0.5f)
//        {
//            Mathf.Lerp(0.33f, 1f, Mathf.InverseLerp(0.5f, 1f, NPC.RelationData.NormalizedRelationDelta));
//        }
//        NPC nPC = null;
//        if (NPC.RelationData.NormalizedRelationDelta >= 0.6f)
//        {
//            nPC = NPC.RelationData.GetLockedDealers(excludeRecommended: true).FirstOrDefault();
//        }
//        NPC nPC2 = null;
//        if (NPC.RelationData.NormalizedRelationDelta >= 0.6f)
//        {
//            nPC2 = NPC.RelationData.GetLockedSuppliers().FirstOrDefault();
//        }
//        string npcToRecommend = string.Empty;
//        if (GameManager.IS_TUTORIAL && NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Completed_Contracts_Count") >= 2.9f)
//        {
//            npcToRecommend = "chelsey_milson";
//        }
//        else if (nPC2 != null)
//        {
//            npcToRecommend = nPC2.ID;
//        }
//        else if (nPC != null)
//        {
//            npcToRecommend = nPC.ID;
//        }
//        ProcessHandoverClient(satisfaction, handoverByPlayer, npcToRecommend);
//    }

//    private void RpcReader___Server_ProcessHandoverServerSide_3760244802(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        HandoverScreen.EHandoverOutcome outcome = GeneratedReaders___Internal.Read___ScheduleOne.UI.Handover.HandoverScreen / EHandoverOutcomeFishNet.Serializing.Generateds(PooledReader0);
//        List<ItemInstance> items = GeneratedReaders___Internal.Read___System.Collections.Generic.List`1 < ScheduleOne.ItemFramework.ItemInstance > FishNet.Serializing.Generateds(PooledReader0);
//        bool handoverByPlayer = PooledReader0.ReadBoolean();
//        float totalPayment = PooledReader0.ReadSingle();
//        ProductList productList = GeneratedReaders___Internal.Read___ScheduleOne.Product.ProductListFishNet.Serializing.Generateds(PooledReader0);
//        float satisfaction = PooledReader0.ReadSingle();
//        NetworkObject dealer = PooledReader0.ReadNetworkObject();
//        if (base.IsServerInitialized)
//        {
//            RpcLogic___ProcessHandoverServerSide_3760244802(outcome, items, handoverByPlayer, totalPayment, productList, satisfaction, dealer);
//        }
//    }

//    private void RpcWriter___Observers_ProcessHandoverClient_537707335(float satisfaction, bool handoverByPlayer, string npcToRecommend)
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            writer.WriteSingle(satisfaction);
//            writer.WriteBoolean(handoverByPlayer);
//            writer.WriteString(npcToRecommend);
//            SendObserversRpc(12u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ProcessHandoverClient_537707335(float satisfaction, bool handoverByPlayer, string npcToRecommend)
//    {
//        TimeSinceLastDealCompleted = 0;
//        if (satisfaction >= 0.5f)
//        {
//            ContractWellReceived(npcToRecommend);
//        }
//        else if (satisfaction < 0.3f)
//        {
//            NPC.PlayVO(EVOLineType.Annoyed);
//        }
//        if (onDealCompleted != null)
//        {
//            onDealCompleted.Invoke();
//        }
//        CurrentContract = null;
//    }

//    private void RpcReader___Observers_ProcessHandoverClient_537707335(PooledReader PooledReader0, Channel channel)
//    {
//        float satisfaction = PooledReader0.ReadSingle();
//        bool handoverByPlayer = PooledReader0.ReadBoolean();
//        string npcToRecommend = PooledReader0.ReadString();
//        if (base.IsClientInitialized)
//        {
//            RpcLogic___ProcessHandoverClient_537707335(satisfaction, handoverByPlayer, npcToRecommend);
//        }
//    }

//    private void RpcWriter___Server_ChangeAddiction_431000436(float change)
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            writer.WriteSingle(change);
//            SendServerRpc(13u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    public void RpcLogic___ChangeAddiction_431000436(float change)
//    {
//        CurrentAddiction = Mathf.Clamp(SyncAccessor_ < CurrentAddiction > k__BackingField + change, customerData.BaseAddiction, 1f);
//        HasChanged = true;
//    }

//    private void RpcReader___Server_ChangeAddiction_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        float change = PooledReader0.ReadSingle();
//        if (base.IsServerInitialized)
//        {
//            RpcLogic___ChangeAddiction_431000436(change);
//        }
//    }

//    private void RpcWriter___Server_RejectProductRequestOffer_2166136261()
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendServerRpc(14u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    public void RpcLogic___RejectProductRequestOffer_2166136261()
//    {
//        RejectProductRequestOffer_Local();
//        if (NPC.responses is NPCResponses_Civilian && NPC.Aggression > 0.1f)
//        {
//            float num = Mathf.Clamp(NPC.Aggression, 0f, 0.7f);
//            num -= NPC.RelationData.NormalizedRelationDelta * 0.3f;
//            num += SyncAccessor_ < CurrentAddiction > k__BackingField * 0.2f;
//            if (UnityEngine.Random.Range(0f, 1f) < num)
//            {
//                NPC.behaviour.CombatBehaviour.SetTarget(null, Player.GetClosestPlayer(base.transform.position, out var _).NetworkObject);
//                NPC.behaviour.CombatBehaviour.Enable_Networked(null);
//            }
//        }
//    }

//    private void RpcReader___Server_RejectProductRequestOffer_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        if (base.IsServerInitialized)
//        {
//            RpcLogic___RejectProductRequestOffer_2166136261();
//        }
//    }

//    private void RpcWriter___Observers_RejectProductRequestOffer_Local_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___RejectProductRequestOffer_Local_2166136261()
//    {
//        NPC.PlayVO(EVOLineType.Annoyed);
//        NPC.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "product_request_fail", 30f, 1);
//        NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "counteroffer_rejected"), 5f);
//    }

//    private void RpcReader___Observers_RejectProductRequestOffer_Local_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized && !base.IsHost)
//        {
//            RpcLogic___RejectProductRequestOffer_Local_2166136261();
//        }
//    }

//    private void RpcWriter___Target_ReceiveCustomerData_2280244125(NetworkConnection conn, ScheduleOne.Persistence.Datas.CustomerData data)
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            GeneratedWriters___Internal.Write___ScheduleOne.Persistence.Datas.CustomerDataFishNet.Serializing.Generated(writer, data);
//            SendTargetRpc(16u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ReceiveCustomerData_2280244125(NetworkConnection conn, ScheduleOne.Persistence.Datas.CustomerData data)
//    {
//        Load(data);
//    }

//    private void RpcReader___Target_ReceiveCustomerData_2280244125(PooledReader PooledReader0, Channel channel)
//    {
//        ScheduleOne.Persistence.Datas.CustomerData data = GeneratedReaders___Internal.Read___ScheduleOne.Persistence.Datas.CustomerDataFishNet.Serializing.Generateds(PooledReader0);
//        if (base.IsClientInitialized)
//        {
//            RpcLogic___ReceiveCustomerData_2280244125(base.LocalConnection, data);
//        }
//    }

//    private void RpcWriter___Server_ProcessSampleServerSide_3704012609(List<ItemInstance> items)
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            GeneratedWriters___Internal.Write___System.Collections.Generic.List`1 < ScheduleOne.ItemFramework.ItemInstance > FishNet.Serializing.Generated(writer, items);
//            SendServerRpc(17u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ProcessSampleServerSide_3704012609(List<ItemInstance> items)
//    {
//        consumedSample = items[0] as ProductItemInstance;
//        NPC.behaviour.ConsumeProductBehaviour.onConsumeDone.AddListener(SampleConsumed);
//        NPC.behaviour.ConsumeProduct(consumedSample);
//        ProcessSampleClient();
//        EndWait();
//    }

//    private void RpcReader___Server_ProcessSampleServerSide_3704012609(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        List<ItemInstance> items = GeneratedReaders___Internal.Read___System.Collections.Generic.List`1 < ScheduleOne.ItemFramework.ItemInstance > FishNet.Serializing.Generateds(PooledReader0);
//        if (base.IsServerInitialized && !conn.IsLocalClient)
//        {
//            RpcLogic___ProcessSampleServerSide_3704012609(items);
//        }
//    }

//    private void RpcWriter___Observers_ProcessSampleClient_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(18u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___ProcessSampleClient_2166136261()
//    {
//        if (!NPC.behaviour.ConsumeProductBehaviour.Enabled && !sampleOfferedToday)
//        {
//            sampleOfferedToday = true;
//            NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "sample_consume_wait"), 5f);
//            NPC.SetAnimationTrigger("GrabItem");
//            NPC.PlayVO(EVOLineType.Think);
//        }
//    }

//    private void RpcReader___Observers_ProcessSampleClient_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized && !base.IsHost)
//        {
//            RpcLogic___ProcessSampleClient_2166136261();
//        }
//    }

//    private void RpcWriter___Observers_SampleWasSufficient_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(19u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___SampleWasSufficient_2166136261()
//    {
//        NPC.PlayVO(EVOLineType.Thanks);
//        NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "sample_sufficient"), 5f);
//        NPC.Avatar.EmotionManager.AddEmotionOverride("Cheery", "sample_provided", 10f);
//        if (!NPC.RelationData.Unlocked)
//        {
//            NPC.RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach);
//        }
//    }

//    private void RpcReader___Observers_SampleWasSufficient_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized)
//        {
//            RpcLogic___SampleWasSufficient_2166136261();
//        }
//    }

//    private void RpcWriter___Observers_SampleWasInsufficient_2166136261()
//    {
//        if (!base.IsServerInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            SendObserversRpc(20u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
//            writer.Store();
//        }
//    }

//    private void RpcLogic___SampleWasInsufficient_2166136261()
//    {
//        NPC.PlayVO(EVOLineType.Annoyed);
//        NPC.dialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Customer, "sample_insufficient"), 5f);
//        NPC.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "sample_insufficient", 5f);
//        if (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("SampleRejectionCount") < 1f && NetworkSingleton<ProductManager>.Instance.onFirstSampleRejection != null)
//        {
//            NetworkSingleton<ProductManager>.Instance.onFirstSampleRejection.Invoke();
//        }
//    }

//    private void RpcReader___Observers_SampleWasInsufficient_2166136261(PooledReader PooledReader0, Channel channel)
//    {
//        if (base.IsClientInitialized)
//        {
//            RpcLogic___SampleWasInsufficient_2166136261();
//        }
//    }

//    private void RpcWriter___Server_AdjustAffinity_3036964899(EDrugType drugType, float change)
//    {
//        if (!base.IsClientInitialized)
//        {
//            NetworkManager networkManager = base.NetworkManager;
//            if ((object)networkManager == null)
//            {
//                networkManager = InstanceFinder.NetworkManager;
//            }
//            if ((object)networkManager != null)
//            {
//                networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//            else
//            {
//                Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
//            }
//        }
//        else
//        {
//            Channel channel = Channel.Reliable;
//            PooledWriter writer = WriterPool.GetWriter();
//            GeneratedWriters___Internal.Write___ScheduleOne.Product.EDrugTypeFishNet.Serializing.Generated(writer, drugType);
//            writer.WriteSingle(change);
//            SendServerRpc(21u, writer, channel, DataOrderType.Default);
//            writer.Store();
//        }
//    }

//    public void RpcLogic___AdjustAffinity_3036964899(EDrugType drugType, float change)
//    {
//        ProductTypeAffinity productTypeAffinity = currentAffinityData.ProductAffinities.Find((ProductTypeAffinity x) => x.DrugType == drugType);
//        productTypeAffinity.Affinity = Mathf.Clamp(productTypeAffinity.Affinity + change, -1f, 1f);
//        HasChanged = true;
//    }

//    private void RpcReader___Server_AdjustAffinity_3036964899(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
//    {
//        EDrugType drugType = GeneratedReaders___Internal.Read___ScheduleOne.Product.EDrugTypeFishNet.Serializing.Generateds(PooledReader0);
//        float change = PooledReader0.ReadSingle();
//        if (base.IsServerInitialized)
//        {
//            RpcLogic___AdjustAffinity_3036964899(drugType, change);
//        }
//    }

//    public virtual bool ReadSyncVar___ScheduleOne.Economy.Customer(PooledReader PooledReader0, uint UInt321, bool Boolean2)
//    {
//        switch (UInt321)
//        {
//            case 1u:
//                {
//                    if (PooledReader0 == null)
//                    {
//                        this.sync___set_value_ < HasBeenRecommended > k__BackingField(syncVar___ < HasBeenRecommended > k__BackingField.GetValue(calledByUser: true), asServer: true);
//                        return true;
//                    }
//                    bool value2 = PooledReader0.ReadBoolean();
//                    this.sync___set_value_ < HasBeenRecommended > k__BackingField(value2, Boolean2);
//                    return true;
//                }
//            case 0u:
//                {
//                    if (PooledReader0 == null)
//                    {
//                        this.sync___set_value_ < CurrentAddiction > k__BackingField(syncVar___ < CurrentAddiction > k__BackingField.GetValue(calledByUser: true), asServer: true);
//                        return true;
//                    }
//                    float value = PooledReader0.ReadSingle();
//                    this.sync___set_value_ < CurrentAddiction > k__BackingField(value, Boolean2);
//                    return true;
//                }
//            default:
//                return false;
//        }
//    }

//    protected virtual void Awake_UserLogic_ScheduleOne.Economy.Customer_Assembly-CSharp.dll()

//    {
//        _ = AvailableInDemo;
//        NPC = GetComponent<NPC>();
//        CurrentAddiction = customerData.BaseAddiction;
//        ScheduleOne.Economy.CustomerData obj = customerData;
//        obj.onChanged = (Action)Delegate.Combine(obj.onChanged, (Action)delegate
//        {
//            HasChanged = true;
//        });
//        currentAffinityData = new CustomerAffinityData();
//        customerData.DefaultAffinityData.CopyTo(currentAffinityData);
//        NPC.ConversationCategories.Add(EConversationCategory.Customer);
//        InitializeSaveable();
//    }
//}
