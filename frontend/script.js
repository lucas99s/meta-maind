const apiChatUrl = "https://localhost:7018/api/openai/chat"; // Endpoint do Chat
const apiUploadUrl = "https://localhost:7018/api/openai/upload"; // Endpoint de Upload

let chatHistory = []; // Armazena o histórico da conversa

async function sendMessage() {
    const messageInput = document.getElementById("messageInput");
    const messagesDiv = document.getElementById("messages");

    const userMessage = messageInput.value.trim();
    if (!userMessage) return;

    // Adiciona a mensagem do usuário ao histórico e ao chat
    chatHistory.push({ role: "user", content: userMessage });
    messagesDiv.innerHTML += `<p><strong>Você:</strong> ${userMessage}</p>`;

    try {
        // Envia o histórico da conversa junto com a nova mensagem
        const response = await fetch(apiChatUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                message: userMessage,  // Envia apenas a mensagem atual
                history: chatHistory,  // Envia o histórico separado
                maxTokens: 1000,
                temperature: 0.7
            })
        });

        // Verifica se a resposta é válida
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(`Erro na API: ${errorMessage}`);
        }

        const data = await response.json();

        // Verifica se a resposta contém `choices`
        if (!data.choices || data.choices.length === 0) {
            throw new Error("A resposta da IA não contém dados válidos.");
        }

        // Obtém a resposta do bot
        const botMessage = data.choices[0].message?.content || "Erro: Resposta vazia da IA.";
        
        // Adiciona ao histórico e exibe no chat
        chatHistory.push({ role: "assistant", content: botMessage });
        messagesDiv.innerHTML += `<p><strong>Bot:</strong> ${botMessage}</p>`;

    } catch (error) {
        console.error("Erro ao processar a resposta da IA:", error);
        messagesDiv.innerHTML += `<p><strong>Bot:</strong> Ocorreu um erro ao processar sua solicitação.</p>`;
    }

    // Limpa o input
    messageInput.value = "";
}

async function uploadImage() {
    const fileInput = document.getElementById("fileInput");
    const messagesDiv = document.getElementById("messages");

    if (fileInput.files.length === 0) {
        alert("Selecione uma imagem para fazer o upload!");
        return;
    }

    const file = fileInput.files[0];
    const formData = new FormData();
    formData.append("file", file); // O nome "file" deve ser o mesmo do parâmetro na Controller

    try {
        // Faz a requisição de upload
        const response = await fetch(apiUploadUrl, {
            method: "POST",
            body: formData
        });

        const data = await response.json();

        if (response.ok) {
            messagesDiv.innerHTML += `<p><strong>Você:</strong> Enviou a imagem <i>${data.fileName}</i></p>`;
            messagesDiv.innerHTML += `<p><strong>Bot:</strong> Upload concluído com sucesso!</p>`;
        } else {
            messagesDiv.innerHTML += `<p><strong>Bot:</strong> Erro ao fazer upload: ${data.message}</p>`;
        }
    } catch (error) {
        messagesDiv.innerHTML += `<p><strong>Bot:</strong> Erro ao conectar ao servidor.</p>`;
    }

    // Limpa o input de arquivo
    fileInput.value = "";
}
