namespace Bounds
{
    internal interface IBounds
    {
        object GetCurrentBounds();
        void RegisterNewBounds(object data);
        void StopRegisteringNewBounds();
        void StartRegisteringNewBounds();
    }
}