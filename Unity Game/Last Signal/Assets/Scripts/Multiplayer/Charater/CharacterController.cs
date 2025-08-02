using System.Globalization;
using TMPro;
using Unity.Multiplayer.Center;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
//using UnityEngine.Windows;

public class CharacterController : NetworkBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Zemin Kontrolü")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    private Rigidbody rb;
    private bool isGrounded;

    // Sadece sunucu ve kendi istemcimiz üzerinde çalýþmasý gereken deðiþkenler
    private Vector2 moveInput;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMPro.TextMeshProUGUI winText;
    bool isWalking;
    Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        // Eðer bu karakter bizim kontrolümüzdeki bir karakter deðilse,
        // klavye giriþlerini dinlemesine gerek yok.
        if (!IsOwner)
        {
            enabled = false; // Script'i devre dýþý býrakarak Update metodunun çalýþmasýný engelleriz.
            return;
        }
    }

    void Update()
    {
        // Sadece kendi kontrol ettiðimiz karakter hareket etmeli
        if (!IsOwner) return;

        // --- Hareket Giriþi ---
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        // --- Zýplama Giriþi ---
      /*  if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Zýplama komutunu sunucuya gönder (Sunucu yetkili bir ortamda)
            RequestJumpServerRpc();
        }*/
    }

    void FixedUpdate()
    {
        // Sadece kendi kontrol ettiðimiz karakter hareket etmeli
        if (!IsOwner) return;

        MoveServerRpc();
        // --- Zemin Kontrolü ---
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        isWalking = moveInput.magnitude > 0.1f;
        animator.SetBool("walk", isWalking);
        // --- Hareket Uygula ---

    }
    [ServerRpc]
    void MoveServerRpc()
    {
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }
    // --- Zýplama Ýþlemi ---
    // Bir RPC (Remote Procedure Call) kullanarak zýplama komutunu sunucuya göndeririz.
    // RPC'ler, istemciden sunucuya (ServerRpc) veya sunucudan istemciye (ClientRpc)
    // fonksiyon çaðrýlarý yapmamýzý saðlar.
    [ServerRpc]
    private void RequestJumpServerRpc()
    {
        // Bu kod sunucuda çalýþýr. Sunucu yetkili olduðundan, zýplamayý burada uygularýz.
        // Daha karmaþýk senaryolarda, burada yetkilendirme veya hile kontrolü yapýlabilir.
        if (isGrounded) // Sunucu tarafýnda da zeminde olup olmadýðýný kontrol etmek önemlidir.
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        // Ýstemcilere zýplama animasyonunu veya sesini oynatmalarý için bilgi gönderebiliriz.
        // Örneðin: PlayJumpAnimationClientRpc();
    }

    // --- Görsel Hata Ayýklama ---
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
  /*  public NetworkVariable<string> PlayerName = new NetworkVariable<string>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Ýsmi bir yerden al, örneðin giriþ ekranýndan
            PlayerName.Value = PlayerPrefs.GetString("PlayerName", "Unknown");
        }
    }

  

   

    [ServerRpc(RequireOwnership = false)]
    public void DeclareWinnerServerRpc(ulong winnerId, string winnerName)
    {
        DeclareWinnerClientRpc(winnerName);
    }

    [ClientRpc]
    private void DeclareWinnerClientRpc(string winnerName)
    {
        winPanel.SetActive(true);
        winText.text = $"Kazanan: {winnerName}";
    }

    private void OnTriggerEnter(Collider other)
    {


        if (other.CompareTag("Flag"))
        {
            winPanel = transform.Find("WinPanel").gameObject;
            winText = winPanel.transform.GetComponentInChildren<TextMeshProUGUI>();
            ulong playerId = other.GetComponent<NetworkObject>().OwnerClientId;
            string playerName = PlayerName.Value;


            DeclareWinnerServerRpc(playerId, playerName);
        }
    }*/
}
