using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using Photon.Pun;

/// <summary>
/// 세줄요약
/// 덱, 플레이어, 딜러에 넣어줄 스크립트
/// </summary>
public class MultiCardStack : MonoBehaviourPunCallbacks
{
    //arraylist 좀 히튼데
    //한번에 여러 자료형을 담을수있는 리스트인 대신 좀 느림

    //덱은 모든 카드값을 다 담은 리스트
    //플레이어와 딜러는 뽑은 카드값 리스트
    List<int> cards;

    //인스펙터에서 덱만 트루로
    public bool isGameDeck;

    MultiCardStackView multicardStackView;

    //델리게이트는 메서드에 대한 참조를 저장하는 특별한 형식의 변수
    // 버튼 클릭과 같은 이벤트를 처리할 때 이벤트 핸들러 메서드의 시그니처는
    // 보통 object cardOwner, EventArgs e와 같이 되며
    // EventArgs는 이벤트 데이터를 포함하지 않는 이벤트에 사용된다.
    public delegate void CardEvent(object cardOwner, CardEventArgs e);

    //action은 매개변수가 없고 값을 반환하지 않는 함수를 캡슐화할때 쓰는것
    //단 이런식으로 제네릭 안에 미리 정의 한채로 만들면 반환값이 없는건 사용가능
    public event Action<object, CardEventArgs> CardRemoved;

    public event CardEvent CardAdded;


	new void OnEnable() 
    {
        cards = new List<int>();
        multicardStackView = GetComponent<MultiCardStackView>();

        //덱만 실행해야함
        //덱 인스펙터창 isGameDeck만 트루로
        if (PhotonNetwork.IsMasterClient && isGameDeck)
        {
            photonView.RPC("CreateDeck", RpcTarget.MasterClient);
        }
	}

    //유용한 프로퍼티들
    public bool HasCards
    {
        get { return cards != null && cards.Count > 0; }
    }

    /// <summary>
    /// 널일때 0반환
    /// </summary>
    public int CardCount

    {
        get
        {
            //카운트류들은 이 프로퍼티를 달아주는게
            //무조건 좋을듯
            if (cards == null)
            {
                return 0;
            }
            else
            {
                return cards.Count;
            }
        }
    }

    [PunRPC]
    public void CreateDeck()
    {
        //덱 리스트 날리기
        cards.Clear();

        //덱 리스트에 값 넣어주기
        //0~51
        //awake에서 한번 돌리기때문에 리스트 널안됨
        for (int i = 0; i < 52; i++)
        {
            cards.Add(i);
        }

        //덱 셔플
        for (int n = cards.Count - 1; n > 1; n--)
        {
            //변수하나 더 만들어서 값 저장해놓고
            //두번 섞음
            int k = Random.Range(0, n + 1);
            int l = Random.Range(0, n + 1);
            int savek = cards[k];
            cards[k] = cards[n];
            cards[n] = cards[l];
            cards[l] = savek;
        }
        multicardStackView.MakeDeckAndFaceUpUpdate();
    }



    /// <summary>
    /// 보안을 위해 리스트 원본은 private로 두고
    /// 이걸 다른 스크립트에서 사용
    /// </summary>
    /// <returns></returns>
    public IEnumerable<int> GetCards()
    {
        //다른곳에서도 열거형으로 사용하고싶을때 IEnumerable<> 제네릭으로 넣고싶은 타입
        //넣어서 쓸수있고 스스로 만든 클래스에 IEnumerable인터페이스 구현해서 쓰면됨
        //ex public IEnumerator GetEnumerator()
        //   {return 반환하고싶은 IEnumerator변수.GetEnumerator();}
        foreach (int i in cards)
        {
            yield return i;
        }
    }


    /// <summary>
    /// 덱의 첫번째 카드 인덱스 리턴 후 삭제
    /// </summary>
    /// <returns></returns>
    public int DeckTop()
    {
        //첫번째 인덱스값 저장
        int temp = cards[0];
        //첫번째 인덱스 삭제
        cards.RemoveAt(0);

        //null 조건 연산자 ?를 사용해 안전하게 이벤트 핸들러 호출
        //연산자의 왼쪽이 null이 아닌 경우에만 실행
        //모든 델리게이트와 이벤트는 invoke메서드를 타입 안정적 형태로 생성해줌
        //()를 이용하여 이벤트를 직접 발생시키는 코드와 동일
        //?이전 문장이 한번 실행되어 임시 변수에 자동 할당되기 때문에
        //Thread Safety보장

        //이 스크립트에선 이벤트에 한개의 콜백밖에 들어가지않았지만
        //기본적으로 이벤트가 여러 콜백들을 동시에 처리하려고 쓰는것도 있으니까
        //내가 원하는 콜백이 등록됐는지 확인하는것도 중요할것

        //첫번째 인덱스의 카드 Destroy
        CardRemoved?.Invoke(this, new CardEventArgs(temp));
        
        // int cnt = row?.Count ?? 0;
        // ??로 널일때 값도 정해줄수있음

        return temp;
    }

    /// <summary>
    /// 덱의 첫번째 카드의 값을 받아와서 생성
    /// </summary>
    /// <param name="card"></param>
    public void Draw(int card)
    {

        cards.Add(card);

        //델리케이트를 지역변수안에 담음으로서
        //이벤트 핸들러가 결합되지 않으면 똑같이 null이고
        //이 스크립트에선 CardAdded에 하나의 메서드밖에 담지않은
        //유니캐스트 델리게이트지만
        //멀티캐스트 델리게이트로 수정되었을때 마지막으로 호출된 대상 함수의 반환값이 반환되므로
        //다른 스레드에서 CardAdded가 수정되더라도
        //지연변수의 내용은 복사가 수행된 시점에 이벤트 핸들러의
        //존재여부를 확인 하게 되어 정상 호출됨. 안정성 증가
        var realCardAdded = CardAdded;
        if (realCardAdded != null)
        {
            //card인덱스 카드 추가
            realCardAdded(this, new CardEventArgs(card));
        }
    }

    /// <summary>
    /// 토탈값 반환
    /// </summary>
    /// <returns></returns>
    public int HandValue()
    {
        //점수
        int total = 0;
        //A 갯수
        int ace = 0;

        foreach (int card in GetCards())
        {
            //한 문양에 13카드씩 2~A까지
            //0~12까지는 하트 13~25까지 다이아
            int cardRank = card % 13;

            if (cardRank == 12)
            {
                //A
                ace++;
                
            }
            else if (9 <= cardRank)
            {
                //J,Q,K
                cardRank = 10;
                total += cardRank;
            }
            else
            {
                //2,3,4,5,6,7,8,9,10
                cardRank += 2;
                total += cardRank;
            }
        }

        //토탈값을 확인해 A값 결정
        for (int i = 0; i < ace; i++)
        {
            if (total + 11 <= 21)
            {
                total = total + 11;
            }
            else
            {
                total = total + 1;
            }
        }

        return total;
    }

    public void Reset()
    {
        cards.Clear();
    }

    public bool HasCard(int cardId)
    {
        //FindIndex는 주어진 조건을 만족하는 첫번째 인덱스값을 반환하는 메서드
        //predicate<int>를 사용해 콜백으로 값을 건내줌
        //해당하는 값이 없으면 -1을 반환하므로 false
        //리스트에 찾는 값이 존재하면 0보다 크거나 같을것이기 때문에
        //true를 반환하여 리스트에 값이 존재하는지를 체크
        return cards.FindIndex(i => i == cardId) >= 0;
    }
}
