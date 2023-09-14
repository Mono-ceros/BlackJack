//마이크로소프트에서 개발한 윈도우 프로그램 개발 환경인 .net framework
//.net의 만개가 넘는 클래스들을 충돌없이 사용하기 위해 사용하는것이 네임스페이스
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

//컴포넌트를 추가해주고 지켜줌
//[RequireComponent(typeof(CardStack))]
public class MultiCardStackView : MonoBehaviourPunCallbacks
{
    //함수 이름이 직관적이냐 아니냐 이것만으로도
    //코드를 읽는데 걸리는 시간이 엄청나게 차이나는듯

    //덱이랑 P,D랑 쓰는 함수가 거의 정해져있어서
    //스크립트를 분리해도 될거같긴한데

    //CardStack이랑 CardStackView는 한 오브젝트에 같이 넣어줌
    //덱의 CardStack, 플레이어의 CardStack, 딜러의 CardStack
    MultiCardStack deck;

    //Toggle에서 isfaceUp Control용
    Dictionary<int, CardView> faceUpControl;
    
    //덱 플레이어 딜러의 카드 위치. 인스펙터창에서 수치 변경
    public Vector3 start;

    //카드간 간격, 인스펙터에서 덱이랑 플레이어 값 다르게 줌
    public float cardOffset;

    //인스펙터창에서 제어
    //기본값 초기화만 해놓은것
    public bool makefaceUp = false;
    public bool decksorting = false;

    //카드프리팹 할당해주기
    public GameObject cardPrefab;

    new void OnEnable()
    {
        faceUpControl = new Dictionary<int, CardView>();
        deck = GetComponent<MultiCardStack>();
        MakeDeckAndFaceUpUpdate();

        //이벤트에 콜백함수 추가
        deck.CardRemoved += Deck_CardRemoved;
        deck.CardAdded += P_D_CardAdded;
    }

    /// <summary>
    /// cardview의 IsFaceUp을 바꿈
    /// IsFaceUp을 근거로 AddCard에서 앞뒷면 결정
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isFaceUp"></param>
    public void Toggle(int card, bool isFaceUp)
    {
        faceUpControl[card].IsFaceUp = isFaceUp;

    }

    //덱, 플레이어, 딜러 하나씩 호출해서 초기화시켜야함
    public void Clear()
    {
        //리스트 클리어
        deck.Reset();

        foreach (CardView view in faceUpControl.Values)
        {
            //CardView의 카드오브젝트 디스트로이
            Destroy(view.Card);
        }

        //딕셔너리 클리어
        faceUpControl.Clear();
    }


    #region 이벤트 안에 넣을 메소드
    void P_D_CardAdded(object cardOwner, CardEventArgs e)
    {
        //카드간 간격
        float co = cardOffset * deck.CardCount;
        //생성된 카드 위치
        Vector3 temp = start + new Vector3(co, 0f);
        //realCardIndex 카드 생성
        AddCard(temp, e.realCardIndex, deck.CardCount);
    }

    void Deck_CardRemoved(object cardOwner, CardEventArgs e)
    {
        //키에 값이 존재하면
        if (faceUpControl.ContainsKey(e.realCardIndex))
        {
            //카드 파괴(여기 있는 카드를 파괴하고 플레이어에게 생성해 카드를 뽑은것을 구현)
            Destroy(faceUpControl[e.realCardIndex].Card);

            faceUpControl.Remove(e.realCardIndex);
        }
    }
    #endregion

    //덱에 있는 이게 널을 뱉는거같은데
    //업데이트 돌렸을때는 멀쩡했는데
    public void MakeDeckAndFaceUpUpdate()
    {
        int cardCount = 0;

        //리스트에 값이 존재하면
        if (deck.HasCards)
        {
            foreach (int i in deck.GetCards())
            {
                float co = cardOffset * cardCount;
                Vector3 temp = start + new Vector3(co, 0f);
                AddCard(temp, i, cardCount);
                cardCount++;
            }
        }
    }

    /// <summary>
    /// 카드를 만들고 딕셔너리 등록
    /// 이미 만들어진 카드들 IsFaceUp 체크해서 스프라이트 앞뒷면 뒤집어줌
    /// </summary>
    /// <param name="position"></param>
    /// <param name="cardIndex"></param>
    /// <param name="positionalIndex"></param>
    void AddCard(Vector3 position, int cardIndex, int positionalIndex)
    {
        //밑에서 카드 생성이 완료되면 딕셔너리에 cardIndex를 키값으로 추가하기때문에
        //딕셔너리에 카드키가 존재면 이미 있는카드, 또 만들면 안되기 때문에 걸러야함
        if (faceUpControl.ContainsKey(cardIndex))
        {
            //뒷면으로 만들어진것들 IsFaceUp 체크해서 뒤집어야 할때 뒤집어줌
            //ToggleFace가 필요할때 호출
            if (!makefaceUp)
            {
                CardModel model = faceUpControl[cardIndex].Card.GetComponent<CardModel>();
                model.ToggleFace(faceUpControl[cardIndex].IsFaceUp);
            }
            return;
        }

        //여기서 카드 Instantiate
        GameObject cardCopy = Instantiate(cardPrefab);
        cardCopy.transform.position = position;

        //만들어진 카드 오브젝트의 카드 모델 스크립트 참조
        CardModel cardModel = cardCopy.GetComponent<CardModel>();
        //모델 스프라이트 번호 전달
        //리스트에서 인덱스를 섞어놔도 스프라이트는 잘 저장됨
        cardModel.cardIndex = cardIndex;
        //플레이어는 makefaceUp이 트루라 앞면
        //덱과 딜러는 뒤집어져있어야하니 펄스 뒷면
        cardModel.ToggleFace(makefaceUp);

        //만들어진 카드 오브젝트의 스프라이트렌더러 참조
        SpriteRenderer spriteRenderer = cardCopy.GetComponent<SpriteRenderer>();

        //그냥 만들면 스프라이트가 겹쳐서 카드 순서가 뒤죽박죽이됨
        //sortingOrder로 sorting Layer를 설정
        //숫자가 큰놈이 위로 감
        //리스트 카운트로 레이어 설정해서 나중에 오는 놈이 위로오도록
        if(decksorting)
        {
            //덱은 리스트 0번인덱스부터 뽑아서 위치 반대로 놔야 자연스러움
            spriteRenderer.sortingOrder = deck.CardCount - positionalIndex;
        }
        else
        {
            spriteRenderer.sortingOrder = positionalIndex;
        }
        


        //애드된 카드 오브젝트를 딕셔너리에 추가
        faceUpControl.Add(cardIndex, new CardView(cardCopy));
    }
}
