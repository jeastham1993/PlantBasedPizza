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
} from "@mui/joy";
import Table from "@mui/joy/Table";
import Moment from "moment";
import { kitchenApi } from "../../axiosConfig";
import { useNavigate } from "react-router-dom";

function KitchenDashboard() {
  const [newOrders, setNewOrders] = useState([]);
  const [preparingOrders, setPreparingOrders] = useState([]);
  const [bakingOrders, setBakingOrders] = useState([]);
  const [qualityCheckOrders, setQualityCheckOrders] = useState([]);
  const [viewedOrder, setViewedOrder] = useState({ itemsOnOrder: [] });
  const [modalOpen, setModalOpen] = React.useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    console.log(localStorage.getItem("staffToken"));

    const staffToken = localStorage.getItem("staffToken");

    if (staffToken === undefined || staffToken === null) {
      navigate("/admin/login");
    }

    const fetchData = async () => {
      try {
        const response = await kitchenApi.get(`/new`);
        setNewOrders(response.data);
      } catch (error) {
        console.error("Error fetching orders:", error);
      }
      try {
        const preparingResponse = await kitchenApi.get(`/prep`);
        setPreparingOrders(preparingResponse.data);
      } catch (error) {}
      try {
        const bakingResponse = await kitchenApi.get(`/baking`);
        setBakingOrders(bakingResponse.data);
      } catch (error) {}
      try {
        const setQualityCheckingResponse = await kitchenApi.get(
          `/quality-check`
        );
        setQualityCheckOrders(setQualityCheckingResponse.data);
      } catch (error) {}
    };
    fetchData();
  }, []);

  async function refreshOrders() {
    try {
      const response = await kitchenApi.get(`/new`);
      setNewOrders(response.data);
      const preparingResponse = await kitchenApi.get(`/prep`);
      setPreparingOrders(preparingResponse.data);
      const bakingResponse = await kitchenApi.get(`/baking`);
      setBakingOrders(bakingResponse.data);
      const setQualityCheckingResponse = await kitchenApi.get(`/quality-check`);
      setQualityCheckOrders(setQualityCheckingResponse.data);
    } catch (error) {
      console.error("Error fetching orders:", error);
    }
  }

  async function prepComplete(order) {
    // Make request to add item to order
    await kitchenApi.put(`/${order.orderIdentifier}/preparing`, {});

    refreshOrders();
  }

  return (
    <Box sx={{ flexGrow: 1 }}>
      <CssBaseline />
      <Container sx={{ py: 1 }} maxWidth="xl">
        <Grid container spacing={1}>
          <Grid item xs={12}>
            <Typography level="h2" gutterBottom>
              New Orders
            </Typography>
            <Sheet>
              <Table>
                <thead>
                  <tr>
                    <th style={{ width: "40%" }}>Order #</th>
                    <th>Received On</th>
                    <th></th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {newOrders.map((order) => (
                    <tr key={order.orderIdentifier}>
                      <td>{order.orderIdentifier}</td>
                      <td>
                        {Moment(order.orderReceivedOn).format(
                          "DD/MM/YYYY HH:mm:ss"
                        )}
                      </td>
                      <td>
                        <Button
                          onClick={() => {
                            console.log(order);
                            setViewedOrder(order);
                            setModalOpen(true);
                          }}
                        >
                          View
                        </Button>
                      </td>
                      <td>
                        <Button
                          onClick={() => {
                            prepComplete(order);
                          }}
                        >
                          Complete
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </Sheet>
          </Grid>
        </Grid>
        <Grid container spacing={1}>
          <Grid item xs={12}>
            <Typography level="h2" gutterBottom>
              Preparing Orders
            </Typography>
            <Sheet>
              <Table>
                <thead>
                  <tr>
                    <th style={{ width: "40%" }}>Order #</th>
                    <th>Received On</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {preparingOrders.map((order) => (
                    <tr key={order.orderIdentifier}>
                      <td>{order.orderIdentifier}</td>
                      <td>
                        {Moment(order.orderReceivedOn).format(
                          "DD/MM/YYYY HH:mm:ss"
                        )}
                      </td>
                      <td>
                        <Button
                          onClick={() => {
                            setViewedOrder(order);
                            setModalOpen(true);
                          }}
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
        <Grid container spacing={1}>
          <Grid item xs={12}>
            <Typography level="h2" gutterBottom>
              Baking Orders
            </Typography>
            <Sheet>
              <Table>
                <thead>
                  <tr>
                    <th style={{ width: "40%" }}>Order #</th>
                    <th>Received On</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {bakingOrders.map((order) => (
                    <tr key={order.orderIdentifier}>
                      <td>{order.orderIdentifier}</td>
                      <td>
                        {Moment(order.orderReceivedOn).format(
                          "DD/MM/YYYY HH:mm:ss"
                        )}
                      </td>
                      <td>
                        <Button
                          onClick={() => {
                            setViewedOrder(order);
                            setModalOpen(true);
                          }}
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
        <Grid container spacing={1}>
          <Grid item xs={12}>
            <Typography level="h2" gutterBottom>
              Quality Check Orders
            </Typography>
            <Sheet>
              <Table>
                <thead>
                  <tr>
                    <th style={{ width: "40%" }}>Order #</th>
                    <th>Received On</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {qualityCheckOrders.map((order) => (
                    <tr key={order.orderIdentifier}>
                      <td>{order.orderIdentifier}</td>
                      <td>
                        {Moment(order.orderReceivedOn).format(
                          "DD/MM/YYYY HH:mm:ss"
                        )}
                      </td>
                      <td>
                        <Button
                          onClick={() => {
                            setViewedOrder(order);
                            setModalOpen(true);
                          }}
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
      <Modal
        aria-labelledby="modal-title"
        aria-describedby="modal-desc"
        open={modalOpen}
        onClose={() => {
          setViewedOrder({ itemsOnOrder: [] });
          setModalOpen(false);
        }}
        sx={{ display: "flex", justifyContent: "center", alignItems: "center" }}
      >
        <Sheet
          variant="outlined"
          sx={{
            maxWidth: "75vw",
            borderRadius: "md",
            p: 3,
            boxShadow: "lg",
          }}
        >
          <Typography
            component="h2"
            id="modal-title"
            level="h4"
            textColor="inherit"
            fontWeight="lg"
            mb={1}
          >
            {viewedOrder.orderIdentifier}
          </Typography>
          {viewedOrder.itemsOnOrder.map((item) => {
            <p>{item.recipeIdentifier}</p>;
          })}
        </Sheet>
      </Modal>
    </Box>
  );
}

export default KitchenDashboard;
