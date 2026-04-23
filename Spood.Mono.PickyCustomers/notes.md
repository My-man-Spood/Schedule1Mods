# Schedule 1 Picky Customers Mod - Key Files and Methods

## Contract Generation and Messaging

### Customer.cs
- **CheckContractGeneration()** (line ~640)
  - The primary method that decides if a customer generates a contract
  - Called during regular gameplay (line ~490)
  - This is where to patch for intercepting contracts and sending custom messages

- **Example of Customer Sending Message** (line ~2443)
  ```csharp
  NPC.MSGConversation.SendMessageChain(NPC.dialogueHandler.Database.GetChain(EDialogueModule.Customer, "offer_expired").GetMessageChain());
  ```

### DealGenerationEvent.cs
- Responsible for contract templates, messaging, and conditions
- **GenerateContractInfo(Customer)** (line ~66)
  - Creates the contract details with product requirements
- **ShouldGenerate(Customer)** (line ~70+)
  - Checks if a contract should be generated based on time and relationship
- **GetRandomRequestMessage()** (line ~85)
  - Selects a message template for the contract request

### MSGConversation.cs
- Represents a conversation thread between an NPC and player
- **Constructor** (line ~107)
  ```csharp
  public MSGConversation(NPC _npc, string _contactName)
  ```
  - Creates new conversation with an NPC and adds to MessagesApp
  
- **SendMessage()** (line ~350-370)
  ```csharp
  public void SendMessage(Message message, bool notify = true, bool network = true)
  ```
  - Adds message to conversation, refreshes UI, handles notifications
  
- **MoveToTop()** (line ~125)
  - Moves conversation to top of conversation list

### Message.cs
- Contains the Message class structure needed for creating messages
- Use with `ESenderType.Other` for NPC-to-player messages

## Picky Customer Implementation Plan

### Patch Strategy
1. Create a Harmony patch for `Customer.CheckContractGeneration()`
2. In PREFIX patch:
   - Check if any product meets minimum enjoyment threshold
   - If none do, return a null contract AND send custom message
   - Return false to skip original method

### Sample Implementation
```csharp
[HarmonyPatch(typeof(Customer), "CheckContractGeneration")]
public class CheckContractGenerationPatch
{
    // Prefix to intercept contract generation
    public static bool Prefix(Customer __instance, ref ContractInfo __result, bool force)
    {
        // Check if any product would meet the customer's enjoyment threshold
        bool hasEnjoyableProduct = false;
        string desiredEffect = ""; // Determine based on customer affinities
        
        // [Product enjoyment calculation logic]
        
        // If no products meet threshold and not forced
        if (!hasEnjoyableProduct && !force)
        {
            // Send custom message instead
            SendPickyCustomerMessage(__instance, desiredEffect);
            
            // Return null contract
            __result = null;
            
            // Skip original method
            return false;
        }
        
        // Otherwise let original method run
        return true;
    }
    
    private static void SendPickyCustomerMessage(Customer customer, string requestedEffect)
    {
        // Get existing conversation or create new one
        MSGConversation conversation = customer.NPC.MSGConversation;
        
        // Create message with custom text based on requested effect
        string messageText = $"Hey, I'm looking for something with a {requestedEffect} effect. Got anything like that?";
        Message message = new Message(Message.ESenderType.Other, messageText);
        
        // Send the message (with notification)
        conversation.SendMessage(message, notify: true, network: false);
    }
}
```

### Notes on Design
- Need to determine desired effect based on customer's highest affinity property
- Should check with minimum enjoyment threshold similar to existing product enjoyment logic
- Consider adding a cooldown system to prevent spam messages
