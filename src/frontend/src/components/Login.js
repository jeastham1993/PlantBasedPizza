import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Input,
  Button,
  FormControl,
  Grid,
} from "@mui/joy";
import { accountApi } from "../axiosConfig";

function Login() {
  const [emailAddress, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (event) => {
    event.preventDefault();
    try {
      console.log('logging in');
      const response = await accountApi.post('/login',
        { emailAddress, password }
      );
      
      const token = response.data.authToken;

      console.log(token);

      localStorage.setItem("token", token); // Save the token in localStorage
      navigate("/"); // Navigate to the home page after successful login
    } catch (err) {
      console.log(err);
      setError("Login failed. Please check your credentials and try again.");
    }
  };

  return (
    <Box component="section" height={"100vh"} width={"100vw"}>
      <Grid container spacing={2}>
        <Grid
          xs={12}
          display="flex"
          justifyContent="center"
          alignItems="center"
          minHeight={"100vh"}
        >
          <Box
            component="form"
            onSubmit={handleSubmit}
            noValidate
            sx={{ mt: 1 }}
          >
            <FormControl required id="emailAddress">
              <Input
                placeholder="Email Address"
                size="lg"
                variant="soft"
                margin="normal"
                fullWidth
                name="emailAddress"
                autoComplete="email"
                autoFocus
                value={emailAddress}
                onChange={(e) => setEmail(e.target.value)}
                sx={{marginBottom: "20px"}}
              />
            </FormControl>
            <FormControl required id="password">
              <Input
                placeholder="Password"
                size="lg"
                variant="soft"
                margin="normal"
                fullWidth
                name="password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </FormControl>
            <Button type="submit" fullWidth sx={{ mt: 3 }}>
              Sign In
            </Button>
            <Button
              variant="outlined"
              component="a"
              href="/register"
              fullWidth
              sx={{ mt: 1, mb: 2 }}
            >
              Register
            </Button>
          </Box>
        </Grid>
      </Grid>
    </Box>
  );
}

export default Login;
