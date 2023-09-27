import React from 'react';
import cs from 'classnames';
import { Form, FormikProvider } from 'formik';
import { Box, Button, Grid, Typography } from '@material-ui/core';

import { Loader, ProgressBar, ShowQuestion } from 'components';
import { LoadingButton } from 'components/Overrides';
import { eSignMsg } from 'constants';
import useStyles from './styles';

export default function RegistrationForm(props) {
  const classes = useStyles();

  const {
    Name,
    isFree,
    formik,
    loading,
    skipAble,
    totalGroups,
    exitHandler,
    activeGroup,
    lastSection,
    handleSubmit,
    saveDisabled,
    firstSection,
    handleSection,
    registrantType,
    saveButtonText,
    allowToPayLater,
    showExit = true,
    isSurvey = false,
    currentQuestions,
    handleRedirection,
    isUpdating = false,
    isUpdatingRegistration = false
  } = props;
  const outerWrapperClass =
    isUpdating || isSurvey
      ? classes.outerWrapper
      : cs(classes.outerWrapper, classes.outerWrapperMarginTop);

  const showMsg =
    lastSection &&
    !isUpdatingRegistration &&
    !isFree &&
    registrantType &&
    registrantType !== 'Volunteer';

  function eSignInfoText() {
    return (
      <Box className={classes.infoText}>
        <Typography data-testid="eSignInfoText">{eSignMsg(allowToPayLater)}</Typography>
      </Box>
    );
  }
  return (
    <FormikProvider value={formik}>
      <Form autoComplete="off" noValidate onSubmit={handleSubmit} className={classes.form}>
        {loading ? (
          <Loader />
        ) : (
          <Box className={outerWrapperClass}>
            <Box className={classes.exitButtonWrapper}>
              {showExit &&
                (isUpdating ? (
                  <Button
                    color="primary"
                    variant="outlined"
                    className={classes.backButton}
                    onClick={() => handleRedirection()}
                    data-testid={`Exit-${Name}`}
                  >
                    Exit
                  </Button>
                ) : (
                  firstSection && (
                    <Button
                      color="primary"
                      variant="outlined"
                      className={classes.backButton}
                      onClick={exitHandler}
                      data-testid={`Exit-${Name}`}
                    >
                      Exit
                    </Button>
                  )
                ))}
            </Box>
            <Box className={classes.progressBar}>
              <ProgressBar activeGroup={activeGroup} totalGroups={totalGroups} />
            </Box>
            <Box className={classes.formWrapper}>
              <Box className={classes.formInnerWrapper}>
                <Typography className={classes.sectionName} variant="h6" color="primary">
                  {Name}
                </Typography>
                <Grid container className={cs(classes.justifyContentCenter, classes.leftPadding)}>
                  <Grid className={classes.innerContainer} item xs={12} md={12} lg={12}>
                    {currentQuestions?.map((question, index) =>
                      ShowQuestion({
                        index,
                        formik,
                        isUpdating,
                        isUpdatingRegistration,
                        currentQuestion: question,
                        currentQuestions: currentQuestions
                      })
                    )}
                  </Grid>
                </Grid>
                <Box className={classes.buttonOuterWrapper}>
                  {showMsg && eSignInfoText()}
                  <Box className={classes.buttonWrapper}>
                    <Button
                      color="primary"
                      variant="outlined"
                      className={classes.backButton}
                      disabled={!activeGroup > 0}
                      onClick={() => handleSection('previous')}
                      data-testid={`Back-${Name}`}
                    >
                      Back
                    </Button>
                    <LoadingButton
                      type="submit"
                      variant="contained"
                      data-testid={`Save-${Name}`}
                      color="primary"
                      text={saveButtonText(lastSection, registrantType, isFree, skipAble)}
                      disabled={saveDisabled}
                      loading={saveDisabled}
                      key={activeGroup}
                      className={classes.saveButton}
                    />
                  </Box>
                </Box>
              </Box>
            </Box>
          </Box>
        )}
      </Form>
    </FormikProvider>
  );
}
