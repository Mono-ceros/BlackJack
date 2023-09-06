using System;

public class CardEventArgs : EventArgs
{
    //프로퍼티로 보안
    public int CardIndex { get; private set; }

    //{ get; init;}
    //init을 사용하면 초기화 이외에 변경불가

    public CardEventArgs(int cardIndex)
    {
        CardIndex = cardIndex;
    }
}