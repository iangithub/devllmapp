from transformers import pipeline

def get_sentiments_with_pipeline(model_name, tokenizer_name, string_arr):
    
    sentiment_analyzer = pipeline(task="sentiment-analysis", 
                                  model=model_name,
                                  tokenizer=tokenizer_name,
                                  return_all_scores=True
                                  )

    # Get sentiments
    results = sentiment_analyzer(string_arr)

    return results


if __name__ == "__main__":
    model_name = "lxyuan/distilbert-base-multilingual-cased-sentiments-student"

    string_arr = [
        "我會披星戴月的想你，我會奮不顧身的前進，遠方煙火越來越唏噓，凝視前方身後的距離",
        "鯊魚寶寶 doo doo doo doo doo doo, 鯊魚寶寶"
    ]

    # 這裡 model_name 和 tokenizer_name 是用一樣的
    predictions = get_sentiments_with_pipeline(
        model_name, model_name, string_arr)
    print(predictions)
