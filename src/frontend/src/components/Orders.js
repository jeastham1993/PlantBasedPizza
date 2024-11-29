import React, { useEffect, useState } from "react";
import {
  Box,
  Container,
  CssBaseline,
  Grid,
  Sheet,
  Typography,
  Button,
  Snackbar,
} from "@mui/joy";
import Table from "@mui/joy/Table";
import { ordersApi } from "../axiosConfig";
import Moment from "moment";
import { NotificationHub } from "./SignalR";

function Orders() {
  const [orders, setOrders] = useState([]);
  const [snackbarOpen, setSnackbarOpen] = React.useState(false);
  const [snackbarContents, setSnackbarContents] = React.useState("");
  const hub = new NotificationHub((message) => {
    console.log("Handling received message:", message);
    setSnackbarContents(message);
    setSnackbarOpen(true);
  });

  useEffect(() => {
    // Replace with your actual endpoint
    const fetchData = async () => {
      try {
        const response = await ordersApi.get(`/`);
        setOrders(response.data);
        console.log(response);
      } catch (error) {
        console.error("Error fetching orders:", error);
      }
    };

    fetchData();
  }, []);

  const handleSnackbarClose = () => {
    setSnackbarContents("");
    setSnackbarOpen(false);
  };

  return (
    <div>
      <Box sx={{ flexGrow: 1 }}>
        <CssBaseline />
        <Container sx={{ py: 1 }} maxWidth="xl">
          <Grid container spacing={1}>
            <Grid item>
              <Typography level="h2" gutterBottom>
                Your Orders
              </Typography>
              <Sheet>
                <Table>
                  <thead>
                    <tr>
                      <th style={{ width: "40%" }}>Order #</th>
                      <th>Order Date</th>
                      <th>Value</th>
                      <th>Item(s)</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {orders.map((order) => (
                      <tr key={order.orderIdentifier}>
                        <td>{order.orderIdentifier}</td>
                        <td>
                          {Moment(order.orderDate).format(
                            "DD/MM/YYYY HH:mm:ss"
                          )}
                        </td>
                        <td>{order.totalPrice}</td>
                        <td>{order.itemCount}</td>
                        <td>
                          <Button
                            component="a"
                            href={`/orders/${order.orderIdentifier}`}
                          >
                            View
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Sheet>
            </Grid>
          </Grid>
        </Container>
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
          {snackbarContents}
        </Snackbar>
      </Box>
    </div>
  );
}

export default Orders;
