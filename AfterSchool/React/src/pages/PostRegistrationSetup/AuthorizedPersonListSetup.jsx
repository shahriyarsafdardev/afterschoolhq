import { Grid, Button, Container, Typography } from '@material-ui/core';
import React, { useEffect, useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';

import { phoneValidationRegex } from 'constants/regex';
import { usePostRegistration } from 'redux/hooks';
import AuthorizedPersonListRow from './AuthorizedPersonListRow';
import AddAuthorizedUserForm from './AddAuthorizedUserForm';
import { useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';
import useStyles from './styles';
import LoadingButton from 'components/Overrides/LoadingButton';
import { isEmpty } from 'lodash';

function AuthorizedPersonListSetup({ authorizedPersons, setActiveStep }) {
  const classes = useStyles();
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
    default: false,
    newObject: true,
    PersonPhoneDataType: 'NotSet'
  };

  const { msgs, updateData } = usePostRegistration();

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    const initialState = {};
    authorizedPersons.forEach(
      (person, index) =>
        (initialState[index] = { ...person, default: true, checked: false, newObject: false })
    );
    setInitialValue(initialState);
  }, [authorizedPersons]);

  const formValidationSchema = () =>
    Yup.object().shape(
      Object.keys(values).reduce((acc, key) => {
        acc[key] = Yup.object().shape({
          Phone: Yup.string().when(['checked'], {
            is: val => val,
            then: () =>
              Yup.string()
                .matches(phoneValidationRegex, 'Phone number is not valid')
                .required('Phone Number is required'),
            otherwise: () => Yup.string().notRequired()
          }),
          FirstName: Yup.string().when(['checked'], {
            is: val => val,
            then: () => Yup.string().required('First Name is required'),
            otherwise: () => Yup.string()
          }),
          LastName: Yup.string().when(['checked'], {
            is: val => val,
            then: () => Yup.string().required('Last Name is required'),
            otherwise: () => Yup.string()
          })
        });

        return acc;
      }, {})
    );

  const formik = useFormik({
    enableReinitialize: true,
    initialValues,
    validationSchema,
    onSubmit: async values => {
      setSaveDisabled(true);
      const filteredAuthorizedPersons = [];
      Object.keys(values).forEach(key => {
        if (values[key].checked) {
          const user = {
            FirstName: values[key].FirstName,
            LastName: values[key].LastName,
            Phone: values[key].Phone,
            PersonPhoneDataType: values[key].PersonPhoneDataType
          };
          filteredAuthorizedPersons.push(user);
        }
      });
      await updateData({
        registrationUid,
        phone: '',
        pinCode: '',
        peopleList: filteredAuthorizedPersons
      }).then(apiErrors => {
        setSaveDisabled(false);
        if (apiErrors.length > 0) return;
        enqueueSnackbar('Submitted.', { variant: 'success' });
        setActiveStep(3);
      });
    }
  });

  const { setFieldValue, values, handleSubmit, errors, touched } = formik;

  useEffect(() => {
    setValidationSchema(formValidationSchema());
  }, [values]);

  const addNewUserHandler = () => {
    const key = Number(Object.keys(values).at(-1)) + 1;
    setFieldValue(key, newUser);
  };

  const handleDeleteField = fieldName => {
    const { [fieldName]: deletedField, ...newValues } = formik.values;
    formik.setValues(newValues);
    formik.setFieldTouched(fieldName, false);
    formik.setFieldError(fieldName, '');
  };

  const saveButtonText =
    !isEmpty(values) && Object.keys(values).some(key => values[key].checked)
      ? 'Save & Notify'
      : 'Save';
  return (
    <Container maxWidth="md">
      <form onSubmit={handleSubmit}>
        <Grid container spacing={2} className={classes.alignLeft}>
          {Object.keys(values).map(key =>
            values[key].default ? (
              <AuthorizedPersonListRow
                person={values[key]}
                key={key}
                handleChange={() =>
                  setFieldValue(key, { ...values[key], checked: !values[key].checked })
                }
                handleEdit={() => setFieldValue(key, { ...values[key], default: false })}
                touched={touched[key]}
                errors={errors[key]}
              />
            ) : (
              <>
                <AddAuthorizedUserForm
                  person={values[key]}
                  setFieldValue={value => {
                    setFieldValue(key, value);
                  }}
                  handleDeleteField={() => handleDeleteField(key)}
                  id={key}
                />
              </>
            )
          )}

          <Grid item xs={12}>
            <Typography className={classes.addPersonButton} onClick={addNewUserHandler}>
              Add another authorized person
            </Typography>
          </Grid>

          <Grid item xs={12} className={classes.listButtonWrapper}>
            <Button
              type="submit"
              variant="outlined"
              color="primary"
              fullWidth
              onClick={() => setActiveStep(3)}
            >
              Cancel
            </Button>
            <LoadingButton
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              text={saveButtonText}
              loading={saveDisabled}
            />
          </Grid>
        </Grid>
      </form>
    </Container>
  );
}

export default AuthorizedPersonListSetup;
