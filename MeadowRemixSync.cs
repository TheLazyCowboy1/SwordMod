
namespace SwordMod;

internal partial class MeadowRemixSync {
  public static void AddOnlineData() {
    if (!IsOnline()) return;
  }

  public static List<Configurable> configs = new();
  //private static Configurable[] configs;
  public static void RegisterConfigsToSync(List<Configurable> _configs) {
    if (!IsOnline()) return;
    configs.Join(_configs);

    
  }
  
  public class RemixData : stuff {
    [OnlineField]
    public static int[] intConfigs;
    [OnlineField]
    public static float[] floatConfigs;
    [OnlineField]
    public static bool[] boolConfigs;
    [OnlineField]
    public static string[] stringConfigs;

    public void AddState() {
    }

    private class State {
      public State() {
        //divide up by type
        List<int> intCfgs = new();
        List<float> floatCfgs = new();
        List<bool> boolCfgs = new();
        List<string> stringCfgs = new();
    
        foreach (Configurable config in configs) {
          switch (config.type) {
            case int: intCfgs.Add(config.value); break;
            case float: floatCfgs.Add(config.value); break;
            case bool: boolCfgs.Add(config.value); break;
            case string: stringCfgs.Add(config.value); break;
          }
        }
        intConfigs = intCfgs.ToArray();
        floatConfigs = floatCfgs.ToArray();
        boolConfigs = boolCfgs.ToArray();
        stringConfigs = stringCfgs.ToArray();

        intCfgs.Clear();
        floatCfgs.Clear();
        boolCfgs.Clear();
        stringCfgs.Clear();
      }
    }
  }
}
