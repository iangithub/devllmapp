使用 KernelMemory + Ollama 實現本地模型與本地向量資料庫Qdrant，實現RAG應用

> 這個範例是使用 Ollama 在本地機器部署 Meta 的 llama3:8b 模型，機器採用的是 8VCPU + 36G RAM 規格，以CPU模式進行（當然有GPU的話，效能會更好），Ollama 是採用 Docker 跑在容器內，以 docker exec -it ollama ollama run llama3:8b 指令下載 llama3:8b 模型並運行，Qdrant向量資料庫同樣以容器方式連行，而 AP 面則使用 Microsoft.KernelMemory.Core 建立 Console 應用，結合 Qdrant向量資料庫，實現RAG應用。