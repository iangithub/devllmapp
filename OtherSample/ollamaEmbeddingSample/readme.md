使用 KernelMemory + Ollama 實作全本地模型（含embedding 模型）與本地向量資料庫Qdrant，實現RAG應用

> 這個範例是使用 Ollama 在本地機器部署 phi3 模型，而embedding 模型則是 snowflake-arctic-embed 模型，
機器採用的是 8VCPU + 36G RAM 規格，以CPU模式進行（當然有GPU的話，效能會更好），Ollama 是採用 Docker 跑在容器內，Qdrant向量資料庫同樣以容器方式連行，而 AP 面則使用 Microsoft.KernelMemory.Core 建立 Console 應用，結合 Qdrant向量資料庫，實現RAG應用。由於Ollama目前並不支援OpenAI的embedding API，因此在實作上必須使用 KernelMemory WithCustomEmbeddingGenerator 方式自建embedding 模型連結以及Ollama的embedding API呼叫。