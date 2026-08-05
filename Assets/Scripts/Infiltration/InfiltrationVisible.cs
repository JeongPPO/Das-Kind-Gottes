using UnityEngine;

namespace Infiltration
{
    public interface IInfiltrationVisible
    {
        // 매니저가 이 오브젝트의 위치를 물어볼 때 사용합니다.
        Vector2Int GridPos { get; }

        // 매니저가 "보여라/숨어라" 명령을 내릴 때 사용합니다.
        void SetVisible(bool visible);
    }
}