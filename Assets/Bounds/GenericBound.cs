namespace Bounds
{
    public abstract class GenericBound : IBounds
    {
        public abstract object GetCurrentBounds();
        public abstract void RegisterNewBounds(object data);

        public abstract void StopRegisteringNewBounds();
    }
}
