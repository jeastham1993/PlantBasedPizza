import React, { useEffect, useState } from "react";
import {
  Box,
  Container,
  CssBaseline,
  Grid,
  Sheet,
  Typography,
  Button,
  ModalDialog,
  ModalClose,
  Modal,
  Snackbar,
} from "@mui/joy";
import Table from "@mui/joy/Table";
import Moment from "moment";
import { ordersAdminApi } from "../../axiosConfig";
import { useNavigate } from "react-router-dom";

function CollectionDashboard() {
  const [collectionOrders, setCollectionOrders] = useState([]);
  const navigate = useNavigate();
  const [snackbarOpen, setSnackbarOpen] = React.useState(false);

  const handleSnackbarClose = () => {
    setSnackbarOpen(false);
  };
  useEffect(() => {
    const staffToken = localStorage.getItem("staffToken");

    if (staffToken === undefined || staffToken === null) {
      navigate("/admin/login");
    }

    const fetchData = async () => {
      try {
        const response = await ordersAdminApi.get(`/awaiting-collection`);
        console.log(response.data);
        setCollectionOrders(response.data);
      } catch (error) {
        console.error("Error fetching orders:", error);
      }
    };

    fetchData();
  }, []);

  async function refreshOrders() {
    try {
      const response = await ordersAdminApi.get(`/awaiting-collection`);
      console.log(response.data);
      setCollectionOrders(response.data);
    } catch (error) {
      console.error("Error fetching orders:", error);
    }
  }

  async function collectOrder(order) {
    // Make request to add item to order
    let submitResponse = await ordersAdminApi.post(`/collected`, {
      OrderIdentifier: order.orderNumber,
    });

    setSnackbarOpen(true);

    refreshOrders();
  }

  return (
    <Box sx={{ flexGrow: 1 }}>
      <CssBaseline />
      <Container sx={{ py: 1 }} maxWidth="xl">
        <Grid container spacing={1}>
          <Grid item xs={12}>
            <Typography level="h2" gutterBottom>
              Awaiting Collection
            </Typography>
            <Sheet>
              <Table>
                <thead>
                  <tr>
                    <th style={{ width: "40%" }}>Order #</th>
                    <th>Order Date</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {collectionOrders.map((order) => (
                    <tr key={order.orderNumber}>
                      <td>{order.orderNumber}</td>
                      <td>
                        {Moment(order.orderDate).format("DD/MM/YYYY HH:mm:ss")}
                      </td>
                      <td>
                        <Button
                          onClick={() => {
                            collectOrder(order);
                          }}
                        >
                          Collect Order
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
      <Box sx={{ width: 500 }}>
        <Snackbar
          autoHideDuration={2000}
          variant="solid"
          color="success"
          anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
          open={snackbarOpen}
          onClose={handleSnackbarClose}
        >
          Collected!
        </Snackbar>
      </Box>
    </Box>
  );
}

export default CollectionDashboard;
