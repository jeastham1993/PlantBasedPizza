// src/App.js
import React, { useEffect, useState } from "react";
import {
  Box,
  Button,
  IconButton,
  Card,
  CardContent,
  Container,
  CssBaseline,
  Grid,
  Typography,
  Link,
  Divider,
  CardActions,
  ListItem,
  AspectRatio,
  Drawer,
  List,
  CardOverflow,
} from "@mui/joy";
import recipeService from "../services/recipeService";
import { ordersApi } from "../axiosConfig";
import { Add, FavoriteBorder, ShoppingCart } from "@mui/icons-material";
import Snackbar from "@mui/joy/Snackbar";

function Home() {
  const [menuItems, setMenuItems] = useState({});
  const [order, setOrder] = useState({ items: [] });
  const [open, setOpen] = React.useState(false);
  const [snackbarOpen, setSnackbarOpen] = React.useState(false);

  const handleSnackbarClose = () => {
    setSnackbarOpen(false);
  };

  const toggleDrawer = (inOpen) => (event) => {
    if (
      event.type === "keydown" &&
      (event.key === "Tab" || event.key === "Shift")
    ) {
      return;
    }

    setOpen(inOpen);
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await recipeService.listRecipes();
        console.log(response);
        const data = response;

        // Group menu items by category
        const groupedData = data.reduce((acc, item) => {
          const category = item.category;
          if (!acc[category]) {
            acc[category] = [];
          }
          acc[category].push(item);
          return acc;
        }, {});

        setMenuItems(groupedData);
      } catch (error) {
        console.error("Error fetching the menu data:", error);
      }
    };

    fetchData();
  }, []);

  function setCategoryName(category) {
    switch (category) {
      case "0":
        return "Pizza";
      case "1":
        return "Sides";
      case "2":
        return "Drinks";
      default:
        return "Other";
    }
  }

  function getImage(category) {
    switch (category) {
      case "0":
        return "/pizza-default.jpg";
      case "1":
        return "/fries.jpg";
      case "2":
        return "/can-default.jpg";
      default:
        return "Other";
    }
  }

  async function addToOrder(item) {
    let orderNumber = order.orderNumber;

    if (orderNumber === undefined) {
      let startOrder = await ordersApi.post("/pickup", {
        customerIdentifier: "",
      });
      orderNumber = startOrder.data.orderNumber;
    }

    let addItemBody = {
      OrderIdentifier: orderNumber,
      RecipeIdentifier: item.recipeIdentifier,
      Quantity: 1,
    };

    // Make request to add item to order
    let addItemResponse = await ordersApi.post(
      `/${orderNumber}/items`,
      addItemBody
    );

    setSnackbarOpen(true);

    setOrder(addItemResponse.data);
  }

  async function submitOrder() {
    let orderNumber = order.orderNumber;

    if (orderNumber === undefined) {
      return;
    }

    // Make request to add item to order
    let submitResponse = await ordersApi.post(`/${orderNumber}/submit`, {
      OrderIdentifier: orderNumber,
      CustomerIdentifier: "",
    });

    setSnackbarOpen(true);

    setOrder({ items: [] });
  }

  return (
    <div>
      <Box sx={{ flexGrow: 1, marginBottom: 0, paddingBottom: 0 }}>
        <Box component="section" height={"100vh"} width={"100vw"}>
          <Grid container spacing={2}>
            <Grid
              xs={12}
              sm={9}
              display="flex"
              justifyContent="center"
              alignItems="center"
              minHeight={"100vh"}
            >
              <Typography
                sx={{ fontSize: "5rem", fontWeight: "700", lineHeight: "1" }}
              >
                Italian Pizza.
                <br />
                Plant Based.
                <br />
                <br />
                Simple.
              </Typography>
            </Grid>
          </Grid>
          <Divider />
        </Box>
        <Box sx={{ width: 500 }}>
          <Snackbar
            autoHideDuration={2000}
            variant="solid"
            color="success"
            anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
            open={snackbarOpen}
            onClose={handleSnackbarClose}
          >
            Added successfully!
          </Snackbar>
        </Box>
        <Drawer
          open={open}
          onClose={toggleDrawer(false)}
          size="md"
          anchor="right"
        >
          <Box
            role="presentation"
            onClick={toggleDrawer(false)}
            onKeyDown={toggleDrawer(false)}
          >
            <List>
              <ListItem>
                <Typography level="h2">Your Order</Typography>
              </ListItem>
              <ListItem>
                <Button
                  color="success"
                  xs={3}
                  style={{ float: "right" }}
                  onClick={() => {
                    submitOrder();
                  }}
                >
                  Submit Order
                </Button>
              </ListItem>
              {order.items.map((item) => (
                <Card
                  sx={{
                    height: "100%",
                    display: "flex",
                    flexDirection: "column",
                  }}
                >
                  <div>
                    <Typography level="title-lg">{item.itemName}</Typography>
                    <Typography level="body-sm">{item.quantity}</Typography>
                  </div>
                </Card>
              ))}
            </List>
          </Box>
        </Drawer>
        <CssBaseline />
        <Container sx={{ pt: 8 }} maxWidth="xl">
          <Grid container spacing={1}>
            <Grid item xs={1}></Grid>
            <Grid item xs={10}>
              {Object.keys(menuItems).map((category) => (
                <div key={category}>
                  <Grid container spacing={4}>
                    {menuItems[category].map((item) => (
                      <Grid
                        item
                        key={item.recipeIdentifier}
                        xs={12}
                        sm={6}
                        md={4}
                      >
                        <Card
                          sx={{
                            maxWidth: "100%",
                            boxShadow: "md",
                            height: "100%",
                          }}
                        >
                          <CardOverflow>
                            <AspectRatio ratio="2">
                              <img
                                src={getImage(category)}
                                loading="lazy"
                                alt=""
                              />
                            </AspectRatio>
                          </CardOverflow>
                          <CardContent>
                            <Typography level="title-md">
                              {item.name}
                            </Typography>
                            <Typography level="body-sm">
                              {item.price.toFixed(2)}
                            </Typography>
                          </CardContent>
                          <CardOverflow>
                            <IconButton
                              variant="solid"
                              color="success"
                              onClick={() => {
                                addToOrder(item);
                              }}
                              sx={{
                                width: "70px",
                                height: "70px",
                                position: "absolute",
                                right: 0,
                                bottom: 0,
                                borderRadius: "80px 0 0 0",
                                zIndex: "100",
                                paddingLeft: "15px",
                                paddingTop: "10px",
                              }}
                            >
                              <Add />
                            </IconButton>
                          </CardOverflow>
                        </Card>
                      </Grid>
                    ))}
                  </Grid>
                  <Divider sx={{ my: 4 }} />
                </div>
              ))}
            </Grid>
          </Grid>
        </Container>
      </Box>
      {order.orderNumber === undefined ? (
        <div></div>
      ) : (
        <div
          style={{
            position: "fixed",
            bottom: "20px",
            right: "20px",
            zIndex: "999",
          }}
        >
          <IconButton
            variant="solid"
            color="primary"
            size="lg"
            onClick={toggleDrawer(true)}
            style={{ height: "100px", width: "100px" }}
          >
            <ShoppingCart />
          </IconButton>
        </div>
      )}
    </div>
  );
}

export default Home;
