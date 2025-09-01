public class AddItemResult
{
    public InventoryState NewState { get; init; }
    public int AddedCount { get; init; }
    
    public AddItemResult(InventoryState newState, int addedCount)
    {
        NewState = newState;
        AddedCount = addedCount;
    }
}

public class RemoveItemResult
{
    public InventoryState NewState { get; init; }
    public int RemovedCount { get; init; }
    
    public RemoveItemResult(InventoryState newState, int removedCount)
    {
        NewState = newState;
        RemovedCount = removedCount;
    }
}