// src/App.js
import React from "react";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import {
  Box,
  Typography,
  CssBaseline,
  Button,
  List,
  Divider,
  ListItem,
  Sheet,
  Avatar,
  IconButton,
  extendTheme,
  ModalClose,
  ListItemButton,
  Container,
} from "@mui/joy";
import Home from "./components/Home";
import Login from "./components/Login";
import OrderDetail from "./components/OrderDetail";
import Orders from "./components/Orders";
import { Drawer } from "@mui/joy";
import { datadogRum } from "@datadog/browser-rum";
import Register from "./components/Register";
import { MenuBook, Menu } from "@mui/icons-material";
import AdminLogin from "./components/admin/AdminLogin";
import KitchenDashboard from "./components/admin/KitchenDashboard";
import CollectionDashboard from "./components/admin/CollectionDashboard";

function App() {
  const [open, setOpen] = React.useState(false);

  const toggleDrawer = (inOpen) => (event) => {
    if (
      event.type === "keydown" &&
      (event.key === "Tab" || event.key === "Shift")
    ) {
      return;
    }

    setOpen(inOpen);
  };

  extendTheme({
    fontFamily: {
      display: "Oxygen", // applies to `h1`–`h4`
      body: "Oxygen", // applies to `title-*` and `body-*`
    },
  });

  return (
    <Sheet>
      <Router>
        <Box sx={{ flexGrow: 1 }}>
          <CssBaseline />
          <div
            style={{
              position: "fixed",
              top: "20px",
              right: "20px",
              zIndex: "999",
            }}
          >
            <IconButton
              variant="outlined"
              color="neutral"
              onClick={toggleDrawer(true)}
              size="lg"
            >
              <Menu />
            </IconButton>
          </div>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              padding: "0 20px",
              borderBottom: "solid px whitesmoke",
            }}
          ></div>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/orders" element={<Orders />} />
            <Route path="/orders/:orderNumber" element={<OrderDetail />} />
            <Route path="/admin/login" element={<AdminLogin />} />
            <Route path="/admin/kitchen" element={<KitchenDashboard />} />
            <Route path="/admin/collection" element={<CollectionDashboard />} />
          </Routes>
          <Box sx={{ bgcolor: "background.paper", pb: 4 }} component="footer">
            <Container maxWidth="lg">
              <Typography variant="h6" align="center" gutterBottom>
                Plant Based Pizza
              </Typography>
              <Typography
                variant="subtitle1"
                align="center"
                color="text.secondary"
                component="p"
              >
                Italian Pizza. Plant Based. Simple.
              </Typography>
              <Box sx={{ mt: 2, display: "flex", justifyContent: "center" }}>
                <Link href="https://www.flaticon.com/free-icons/pizza" variant="body2" sx={{ mx: 2 }}>
                  Pizza icons created by Pause08 - Flaticon | 
                </Link>
                <Link href="https://unsplash.com/photos/pizza-with-green-leaves-on-white-ceramic-plate-FZTwpjRUr38" variant="body2" sx={{ mx: 2 }}>
                  Pizza image attribution - Unsplash | 
                </Link>
                <Link href="https://unsplash.com/photos/french-fries-vi0kZuoe0-8" variant="body2" sx={{ mx: 2 }}>
                  Fries image attribution - Unsplash
                </Link>
              </Box>
            </Container>
          </Box>
          <Drawer open={open} onClose={toggleDrawer(false)}>
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 0.5,
                ml: "auto",
                mt: 1,
                mr: 2,
              }}
            >
              <Typography
                component="label"
                htmlFor="close-icon"
                fontSize="sm"
                fontWeight="lg"
                sx={{ cursor: "pointer" }}
              >
                Close
              </Typography>
              <ModalClose id="close-icon" sx={{ position: "initial" }} />
            </Box>
            <List
              size="lg"
              component="nav"
              sx={{
                flex: "none",
                fontSize: "xl",
                "& > div": { justifyContent: "center" },
              }}
            >
              <ListItemButton onClick={() => (window.location.href = "/")}>
                Home
              </ListItemButton>
              <ListItemButton
                onClick={() => (window.location.href = "/orders")}
              >
                Orders
              </ListItemButton>
              <Divider />
              {localStorage.getItem("token") === "undefined" ? (
                <ListItemButton
                  onClick={() => (window.location.href = "/login")}
                >
                  Login
                </ListItemButton>
              ) : (
                <ListItemButton
                  onClick={() => {
                    console.log(localStorage.getItem("token"));
                    localStorage.setItem("token", undefined);
                    window.location.href = "/";
                  }}
                >
                  Logout
                </ListItemButton>
              )}
            </List>
          </Drawer>
        </Box>
      </Router>
    </Sheet>
  );
}

export default App;
