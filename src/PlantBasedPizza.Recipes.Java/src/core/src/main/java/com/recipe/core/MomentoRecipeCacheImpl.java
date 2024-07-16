package com.recipe.core;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import momento.sdk.CacheClient;
import momento.sdk.auth.CredentialProvider;
import momento.sdk.config.Configurations;
import momento.sdk.responses.cache.GetResponse;
import momento.sdk.responses.cache.SetResponse;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.List;
import java.util.Optional;

@Service
public class MomentoRecipeCacheImpl implements RecipeCache {
    private final CacheClient cacheClient;
    private final String cacheName = System.getenv("CACHE_NAME");
    private final ObjectMapper objectMapper;
    Logger log = LogManager.getLogger(MomentoRecipeCacheImpl.class);
    public MomentoRecipeCacheImpl(){
        this.objectMapper = new ObjectMapper();
        cacheClient = new CacheClient(
                CredentialProvider.fromEnvVar("MOMENTO_API_KEY"),
                Configurations.InRegion.v1(),
                Duration.ofSeconds(300));
    }

    @Override
    public void SetRecipes(List<RecipeDTO> recipes) {
        try {
            log.info("Updating cache...");
            this.Set("all-recipes", this.objectMapper.writeValueAsString(recipes));
        }
        catch (JsonProcessingException ex){
            log.error(ex);
        }
    }

    @Override
    public Optional<List<RecipeDTO>> GetRecipes() {
        var cacheResult = this.Get("all-recipes");

        try {
            if (cacheResult.isPresent()){
                log.info("Cache hit...");
                List<RecipeDTO> recipeList = this.objectMapper.readValue(cacheResult.get(), new TypeReference<>() {});

                return Optional.of(recipeList);
            }
            else {
                return Optional.empty();
            }
        }
        catch (JsonProcessingException e) {
            log.error(e);
        }

        return Optional.empty();
    }

    @Override
    public void SetRecipe(RecipeDTO recipe) {
        try {
            log.info("Updating cache...");
            this.Set(String.valueOf(recipe.getId()), this.objectMapper.writeValueAsString(recipe));
        }
        catch (JsonProcessingException ex){
            log.error(ex);
        }
    }

    @Override
    public Optional<RecipeDTO> GetRecipe(String recipeId) {
        var cacheResult = this.Get(String.valueOf(recipeId));

        try {
            if (cacheResult.isPresent()){
                log.info("Cache hit...");
                RecipeDTO recipe = this.objectMapper.readValue(cacheResult.get(), RecipeDTO.class);

                return Optional.of(recipe);
            }
        }
        catch (JsonProcessingException e) {
            log.error(e);
        }

        return Optional.empty();
    }

    private void Set(String key, String value) {
        var setResponse = cacheClient.set(cacheName, key, value).join();

        if (setResponse instanceof SetResponse.Success) {
            System.out.println("Cached successfully");
        } else if (setResponse instanceof SetResponse.Error error) {
            throw new RuntimeException(
                    "An error occurred while attempting to store key 'test-key' in cache 'test-cache': "
                            + error.getErrorCode(),
                    error);
        }
    }

    private Optional<String> Get(String key) {
        var getResponse = cacheClient.get(cacheName, key).join();

        if (getResponse instanceof GetResponse.Hit hit) {
            System.out.println("Retrieved value for key 'test-key': " + hit.valueString());
            return Optional.of(hit.valueString());
        } else if (getResponse instanceof GetResponse.Miss) {
            System.out.println("Key 'test-key' was not found in cache 'test-cache'");
            return Optional.empty();
        } else if (getResponse instanceof GetResponse.Error error) {
            throw new RuntimeException(
                    "An error occurred while attempting to get from cache: "
                            + error.getErrorCode(),
                    error);
        }
        else {
            return Optional.empty();
        }
    }
}
