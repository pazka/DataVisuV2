namespace Bounds
{
    public abstract class GenericBound : IBounds
    {
        protected bool CanRegisterNewBounds = true;

        public void StopRegisteringNewBounds()
        {
            CanRegisterNewBounds = false;
        }
        public void StartRegisteringNewBounds()
        {
            CanRegisterNewBounds = true;
        }
        public abstract object GetCurrentBounds();
        public abstract void RegisterNewBounds(object data);
    }
}
