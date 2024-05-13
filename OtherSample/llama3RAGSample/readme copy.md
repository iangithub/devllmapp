使用 SK + Ollama 實現本地模型連結

> 這個範例是使用 Ollama 在本地機器部署 Meta 的 llama3:8b 模型，機器採用的是 8VCPU + 36G RAM 規格，以CPU模式進行（當然有GPU的話，效能會更好），Ollama 是採用 Docker 跑在容器內，以 docker exec -it ollama ollama run llama3:8b 指令下載 llama3:8b 模型並運行，AP 面則使用 Semantic Kernel 1.10.0 版本建立一個對話的 Console 應用，程式碼行數約 50 行。