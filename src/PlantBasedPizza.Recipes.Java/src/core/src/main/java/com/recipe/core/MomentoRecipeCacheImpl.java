package com.recipe.core;

import momento.sdk.CacheClient;
import momento.sdk.auth.CredentialProvider;
import momento.sdk.config.Configuration;
import momento.sdk.config.Configurations;
import momento.sdk.responses.cache.GetResponse;
import momento.sdk.responses.cache.SetResponse;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.Optional;

@Service
public class MomentoRecipeCacheImpl implements RecipeCache {
    private final CacheClient cacheClient;
    private final String cacheName = System.getenv("CACHE_NAME");
    public MomentoRecipeCacheImpl(){
        cacheClient = new CacheClient(
                CredentialProvider.fromEnvVar("MOMENTO_API_KEY"),
                Configurations.Laptop.v1(),
                Duration.ofSeconds(300));
    }

    @Override
    public void Set(String key, String value) {
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

    @Override
    public Optional<String> Get(String key) {
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
