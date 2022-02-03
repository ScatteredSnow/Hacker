namespace Hacker
{
    public class API : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Loader.Init();
        }

        public void OnUnityUpdate()
        {
        }
    }
}
