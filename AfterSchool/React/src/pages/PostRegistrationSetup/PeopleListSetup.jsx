import { Grid, Button, Container, Typography, CircularProgress } from '@material-ui/core';
import React, { useEffect, useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { isEmpty } from 'lodash';

import { phoneValidationRegex } from 'constants/regex';
import { usePostRegistration } from 'redux/hooks';
import PeopleListRow from './PeopleListRow';
import AddUserForm from './AddUserForm';
import { useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';

function PeopleListSetup({ listOfPeople, setActiveStep }) {
  const { registrationUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [initialValues, setInitialValue] = useState({});
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [validationSchema, setValidationSchema] = useState({});
  const newUser = {
    FirstName: '',
    LastName: '',
    Phone: '',
    checked: false,
    default: false
  };

  const {
    msgs,
    updateData,
    data: { activityDto }
  } = usePostRegistration();

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    const authorizedPersons = listOfPeople.filter(({ FirstName }) => FirstName !== null);
    const initialState = {};
    authorizedPersons.forEach(
      (person, index) => (initialState[index] = { ...person, default: true, checked: false })
    );
    initialState[authorizedPersons.length] = newUser;
    setInitialValue(initialState);
  }, [listOfPeople]);

  const formValidationSchema = values =>
    Yup.object().shape(
      Object.keys(values).reduce((acc, key) => {
        acc[key] = Yup.object().shape({
          phoneNumber: Yup.string().when('checked', {
            is: true,
            then: Yup.string()
              .matches(phoneValidationRegex, 'Phone number is not valid')
              .required('Phone Number is required'),
            otherwise: Yup.string().notRequired()
          }),
          firstName: Yup.string().when('checked', {
            is: true,
            then: Yup.string().required('First Name is required'),
            otherwise: Yup.string().notRequired()
          }),
          lastName: Yup.string().when('checked', {
            is: true,
            then: Yup.string().required('Last Name is required'),
            otherwise: Yup.string().notRequired()
          })
        });

        return acc;
      }, {})
    );

  useEffect(() => {
    !isEmpty(values) && setValidationSchema(formValidationSchema(values));
  }, [values]);

  const formik = useFormik({
    enableReinitialize: true,
    initialValues,
    // validationSchema,
    onSubmit: async values => {
      setSaveDisabled(true);
      const authorizedPersons = [];
      Object.keys(values).forEach(key => {
        if (values[key].checked) {
          const user = {
            FirstName: values[key].FirstName,
            LastName: values[key].LastName,
            Phone: values[key].Phone
          };
          authorizedPersons.push(user);
        }
      });
      await updateData({
        registrationUid,
        phone: '',
        pinCode: '',
        peopleList: authorizedPersons
      }).then(apiErrors => {
        setSaveDisabled(false);
        if (apiErrors.length > 0) return;
        enqueueSnackbar('Submitted.', { variant: 'success' });
        setActiveStep(3);
      });
    }
  });

  const { setFieldValue, values, handleSubmit, errors, touched } = formik;

  const handleInputChange = (event, id) => {
    const { name, value } = event.target;
    setFieldValue(id, { ...values[id], [name]: name === 'checked' ? !values[id].checked : value });
  };

  const addNewUserHandler = () => {
    const newValues = { ...values };
    const key = Number(Object.keys(values)[-1]) + 1;
    newValues[key] = newUser;
    setInitialValue(newValues);
  };

  return (
    <Container maxWidth="md">
      <form onSubmit={handleSubmit}>
        <Grid container spacing={2} style={{ textAlign: 'left' }}>
          {Object.keys(values).map(key =>
            values[key].default ? (
              <PeopleListRow
                person={values[key]}
                key={key}
                handleChange={() =>
                  setFieldValue(key, { ...values[key], checked: !values[key].checked })
                }
                handlePhoneInputChange={e =>
                  setFieldValue(key, { ...values[key], Phone: e.target.value })
                }
                touched={touched}
                errors={errors}
              />
            ) : (
              <AddUserForm
                values={values[key]}
                handleInputChange={e => handleInputChange(e, key)}
                key={key}
                touched={touched}
                errors={errors}
              />
            )
          )}

          <Grid item xs={12}>
            <Typography
              style={{ textDecoration: 'underline', cursor: 'pointer' }}
              onClick={addNewUserHandler}
            >
              Add another authorized person
            </Typography>
          </Grid>

          <Grid item xs={12} style={{ display: 'flex', gap: '8px' }}>
            <Button type="submit" variant="outlined" color="primary" fullWidth>
              Cancel
            </Button>
            <Button
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              disabled={saveDisabled}
            >
              {saveDisabled ? (
                <>
                  {'Loading . . .'} <CircularProgress size={'20px'} />
                </>
              ) : (
                'Save & Notify'
              )}
            </Button>
          </Grid>
        </Grid>
      </form>
    </Container>
  );
}

export default PeopleListSetup;
