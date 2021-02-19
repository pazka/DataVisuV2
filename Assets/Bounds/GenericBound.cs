namespace Bounds
{
    abstract class GenericBound : IBounds
    {
        public abstract object GetCurrentBounds();
        public abstract void RegisterNewBounds(object data);
    }
}
