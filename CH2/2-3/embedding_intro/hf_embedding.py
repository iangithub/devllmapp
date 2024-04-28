from sentence_transformers import SentenceTransformer
sentences = ["鯊魚寶寶 doo doo doo doo doo doo, 鯊魚寶寶"]

model = SentenceTransformer("sentence-transformers/all-MiniLM-L6-v2")
embeddings = model.encode(sentences)
print("Dimesion: ", len(embeddings[0]))
print(embeddings)
