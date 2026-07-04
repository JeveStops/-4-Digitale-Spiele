using UnityEngine;

public class SpiderLegController : MonoBehaviour
{
    [SerializeField] private SpiderLeg legFL;
    [SerializeField] private SpiderLeg legFR;
    [SerializeField] private SpiderLeg legBL;
    [SerializeField] private SpiderLeg legBR;

    private void Update()
    {
        bool groupAIsStepping = legFL.IsStepping || legBR.IsStepping;
        bool groupBIsStepping = legFR.IsStepping || legBL.IsStepping;

        if (!groupBIsStepping)
        {
            legFL.TryStep();
            legBR.TryStep();
        }

        if (!groupAIsStepping)
        {
            legFR.TryStep();
            legBL.TryStep();
        }
    }
}
