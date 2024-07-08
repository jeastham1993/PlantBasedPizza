package com.recipe.api.configs;

import datadog.trace.api.Trace;
import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.io.Decoders;
import io.jsonwebtoken.security.Keys;
import java.security.Key;
import java.util.Date;
import java.util.function.Function;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.stereotype.Service;

@Service
public class JwtService {
    private String secretKey = System.getenv("JWT_KEY");

    public String extractUsername(String token) {
        return extractClaim(token, Claims::getSubject);
    }

    @Trace(operationName = "extractClaim", resourceName = "JwtService.ExtractClaim")
    public <T> T extractClaim(String token, Function<Claims, T> claimsResolver) {
        final Claims claims = extractAllClaims(token);
        return claimsResolver.apply(claims);
    }

    @Trace(operationName = "isTokenValid", resourceName = "JwtService.IsTokenValid")
    public boolean isTokenValid(String token, UserDetails userDetails) {
        final String username = extractUsername(token);
        return !isTokenExpired(token);
    }

    @Trace(operationName = "isTokenExpired", resourceName = "JwtService.IsTokenExpired")
    private boolean isTokenExpired(String token) {
        return extractExpiration(token).before(new Date());
    }

    private Date extractExpiration(String token) {
        return extractClaim(token, Claims::getExpiration);
    }

    private Claims extractAllClaims(String token) {
        return Jwts
                .parserBuilder()
                .setSigningKey(getSignInKey())
                .build()
                .parseClaimsJws(token)
                .getBody();
    }

    private Key getSignInKey() {
        byte[] keyBytes = secretKey.getBytes();
        return Keys.hmacShaKeyFor(keyBytes);
    }
}